using System.ComponentModel;
using System.Diagnostics;
using KapyTask.Database;
using KapyTask.Database.Tables;

namespace KapyTask.Views.Modals;

/// <summary>
/// Modal for editing task. 
/// </summary>
public partial class TaskEditModal : ContentPage, INotifyPropertyChanged
{
    private KapyTaskDatabase db;

    private KTask _kTask = new();

    public KTask KTask
    {
        get => _kTask;
        set
        {
            _kTask = value;
            OnPropertyChanged();
            
            // Когда KTask меняется, обновляем SelectedDiscipline
            if (value != null && Disciplines != null)
            {
                SelectedDiscipline = Disciplines.FirstOrDefault(d => d.Id == value.DisciplineId);
            }
        }
    }

    private KDiscipline _selectedDiscipline;
    public KDiscipline SelectedDiscipline
    {
        get => _selectedDiscipline;
        set
        {
            _selectedDiscipline = value;
            OnPropertyChanged();
            
            // Когда выбирается дисциплина, обновляем KTask
            if (value != null)
            {
                KTask.DisciplineId = value.Id;
                KTask.Discipline = value;
                SetCurrentScheduleForDiscipline();
                SuggestDeadlineDates(); // Предлагаем даты при выборе дисциплины
            }
        }
    }

    private List<KDiscipline> _disciplines;
    public List<KDiscipline> Disciplines
    {
        get => _disciplines;
        set
        {
            _disciplines = value;
            OnPropertyChanged();
            
            // После загрузки дисциплин устанавливаем выбранную
            if (value != null)
            {
                SelectedDiscipline = value.FirstOrDefault(d => d.Id == KTask.DisciplineId);
            }
        }
    }

    private List<KSchedule> _globalSchedule { get; set; }

    private List<KSchedule> _scheduleForCurrentDiscipline;
    public List<KSchedule> ScheduleForCurrentDiscipline
    {
        get => _scheduleForCurrentDiscipline;
        private set
        {
            _scheduleForCurrentDiscipline = value;
            OnPropertyChanged();
        }
    }

    // Свойства для предложения дат
    private List<DateTime> _suggestedDeadlines;
    public List<DateTime> SuggestedDeadlines
    {
        get => _suggestedDeadlines;
        private set
        {
            _suggestedDeadlines = value;
            OnPropertyChanged();
        }
    }
    
    
    private DateTime _selectedDeadline;
    public DateTime SelectedDeadline
    {
        get => _selectedDeadline;
        set
        {
            _selectedDeadline = value;
            KTask.Deadline = value;
            OnPropertyChanged();
        }
    }

    private bool _isNewTask;
    public bool IsNewTask
    {
        get => _isNewTask;
        set
        {
            _isNewTask = value;
            OnPropertyChanged();
        }
    }

    public TaskEditModal(KTask? givenKTask = null)
    {
        InitializeComponent();
        db = new();
        BindingContext = this; //fall for null

        Task.Run(async () => await LoadDisciplines());
        Task.Run(async () => await LoadSchedule());

        KTask = givenKTask ?? new KTask();

        IsNewTask = givenKTask == null;
    }

    private async Task LoadSchedule()
    {
        _globalSchedule = await db.Schedule.GetSchedule();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SetCurrentScheduleForDiscipline();
            if (SelectedDiscipline != null)
                SuggestDeadlineDates();
        });
    }

    private void SetCurrentScheduleForDiscipline()
    {
        if (SelectedDiscipline is not null && _globalSchedule != null)
        {
            ScheduleForCurrentDiscipline =
                _globalSchedule.Where(schedule => schedule.DisciplineId == SelectedDiscipline.Id).ToList();
        }
    }

    /// <summary>
    /// Suggest deadlines based on selected discipline
    /// </summary>
    private void SuggestDeadlineDates()
    {
        if (ScheduleForCurrentDiscipline == null || !ScheduleForCurrentDiscipline.Any())
        {
            SuggestedDeadlines = new List<DateTime>();
            return;
        }

        var suggestions = new List<DateTime>();
        var currentDate = DateTime.Now;
        var maxDaysToLook = 60; // Watch for 60 days

        for (int i = 0; i < maxDaysToLook; i++)
        {
            var checkDate = currentDate.AddDays(i);
            var checkWeekParity = GetWeekParity(checkDate);
            
            foreach (var schedule in ScheduleForCurrentDiscipline)
            {
                if (checkDate.DayOfWeek == schedule.DayOfWeek.ToSystemDayOfWeek() && 
                    checkWeekParity == schedule.IsEvenWeek)
                {
                    var classDateTime = checkDate.Date.Add(schedule.TimeOfClass.ToTimeSpan());
                    
                    if (classDateTime.Date > currentDate.Date && !suggestions.Contains(classDateTime))
                    {
                        suggestions.Add(classDateTime);
                    }

                    else if (classDateTime.Date == currentDate.Date && classDateTime.TimeOfDay > currentDate.TimeOfDay)
                    {
                        if (!suggestions.Contains(classDateTime))
                            suggestions.Add(classDateTime);
                    }
                }
            }
        }

        SuggestedDeadlines = suggestions
            .OrderBy(d => d)
            .Distinct()
            .Take(5) // Suggest only 5 elements
            .ToList();

        if (KTask.Deadline == null && SuggestedDeadlines.Any())
        {
            SelectedDeadline = SuggestedDeadlines.First();
        }
        else if (KTask.Deadline != null)
            SelectedDeadline = (DateTime)KTask.Deadline;
    }

    /// <summary>
    /// IsEvenWeek based on DateTime?
    /// </summary>
    private bool GetWeekParity(DateTime date)
    {
        // Упрощенный алгоритм: первая неделя сентября - нечетная
        var startOfYear = new DateTime(date.Year, 9, 1);
        int daysDiff = (date - startOfYear).Days;
        
        // Если дата раньше сентября, берем предыдущий год
        if (daysDiff < 0)
        {
            startOfYear = new DateTime(date.Year - 1, 9, 1);
            daysDiff = (date - startOfYear).Days;
        }
        
        int weekNumber = daysDiff / 7;
        return weekNumber % 2 != 1;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
    }

    private async Task LoadDisciplines()
    {
        var disciplines = await db.Disciplines.GetDisciplines();
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Disciplines = disciplines;
        });
    }

    /// <summary>
    /// Dumps current task to database
    /// </summary>
    private async void ButtonConfirm_OnClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KTask.Name))
        {
            await DisplayAlert("Ошибка", "Введите название задачи", "OK");
            return;
        }

        if (IsNewTask)
        {
            await db.Tasks.InsertTask(KTask);
            Debug.WriteLine($"Inserted: {KTask}");
            await Navigation.PopModalAsync();
        }
        else if (await DisplayAlert("Подтверждение", "Сохранить задачу?", "Да", "Нет"))
        {
            await db.Tasks.InsertTask(KTask);
            Debug.WriteLine($"Inserted: {KTask}");
            await Navigation.PopModalAsync();
        }
    }

    private async void ButtonClose_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void PickerDiscipline_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        SetCurrentScheduleForDiscipline();
        SuggestDeadlineDates();
    }

    private void ButtonResetUserPlannedTime_OnClicked(object? sender, EventArgs e)
    {
        KTask.UserPlannedTimeTodo = null;
        OnPropertyChanged();
    }
}