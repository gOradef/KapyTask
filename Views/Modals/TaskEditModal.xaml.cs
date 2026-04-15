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
    private static KapyTaskDatabase? db;

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
                SuggestDeadlineDates();
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

    private DateTime? _displayedDeadline;

    public DateTime? DisplayedDeadline
    {
        get => _displayedDeadline ?? DateTime.Now;
        set
        {
            _displayedDeadline = value;
            OnPropertyChanged();
        }
    }

    private DateTime? _selectedCustomDeadline;
    public DateTime? SelectedCustomDeadline
    {
        get => _selectedCustomDeadline;
        set
        {
            _selectedCustomDeadline = value;
            
            DisplayedDeadline = value;
            KTask.Deadline = value;
            OnPropertyChanged();
        }
    }
    
    private DateTime? _selectedSuggestedDeadline;
    public DateTime? SelectedSuggestedDeadline
    {
        get => _selectedSuggestedDeadline;
        set
        {
            _selectedSuggestedDeadline = value;
            
            DisplayedDeadline = value;
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

    public bool IsOldTask { get => !IsNewTask; }
    
    /// Returns 'new task' or 'apply changes' text
    public string ButtonConfirmText => IsNewTask ? "Создать задачу" : "Применить изменения";

    public TaskEditModal(KTask? givenKTask = null)
    {
        InitializeComponent();
        db = new();
        IsNewTask = givenKTask == null; // DONT MOVE. LOAD IsNewTask before binding,
                                        // so UI(archive task button) will be validly shown.
        BindingContext = this;
        Task.Run(() =>
        {
            KTask = givenKTask ?? new KTask();
        });
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await Task.Run(async () => await LoadDisciplines());
        await Task.Run(async () => await LoadSchedule());
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
            SelectedSuggestedDeadline = SuggestedDeadlines.First();
        }
        else if (KTask.Deadline != null)
            SelectedSuggestedDeadline = (DateTime)KTask.Deadline;
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

    private async Task LoadDisciplines()
    {
        var disciplines = (await db.Disciplines.GetDisciplines())
            .OrderBy(a => a.Name)
            .ToList();
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Disciplines = disciplines;
        });
    }

    /// <summary>
    /// Dumps current task to database
    /// </summary>
    private async void ButtonConfirmChanges_OnClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(KTask.Name))
        {
            await DisplayAlertAsync("Ошибка", "Введите название задачи", "OK");
            return;
        }

        if (IsNewTask)
        {
            await db.Tasks.InsertTask(KTask);
            Debug.WriteLine($"Inserted: {KTask}");
            await Navigation.PopModalAsync();
            return;
        }
        
        if (await DisplayAlertAsync("Подтверждение", "Сохранить задачу?", "Да", "Нет"))
        {
            await db.Tasks.InsertTask(KTask);
            Debug.WriteLine($"Inserted: {KTask}");
            await Navigation.PopModalAsync();
            return;
        }
    }
    
    /// <summary>
    /// Should not be able to execute if IsNewTask
    /// </summary>
    private async void MaterialButtonArchiveTask_OnClicked(object? sender, EventArgs e)
    {
        if (await DisplayAlertAsync("Подтверждение", "Архивировать задачу?", "Да", "Нет"))
        {
            await db.ArchivedTasks.ArchiveTask(KTask);
            Debug.Write(KTask);
            return;
        }
    }

    private void ButtonClose_OnClicked(object? sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void PickerDiscipline_OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        SetCurrentScheduleForDiscipline();
        SuggestDeadlineDates();
    }

    private void DatePicker_OnCustomDateSelected(object? sender, DateChangedEventArgs e)
    {
        var picker = sender as DatePicker;
        var date = picker?.Date;
        if (date != null)
            SelectedCustomDeadline = (DateTime)date;
    }
}