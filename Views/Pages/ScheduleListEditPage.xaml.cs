using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using KapyTask.Database;
using KapyTask.Database.Tables;

namespace KapyTask.Views.Modals;

public partial class ScheduleListEditPage : ContentPage, INotifyPropertyChanged
{
    private KapyTaskDatabase db;

    private List<KSchedule> _displayedSchedule = new();

    public List<KSchedule> DisplayedSchedule
    {
        get => _displayedSchedule;
        set
        {
            _displayedSchedule = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<KDiscipline> Disciplines { get; } = new();

    // Create new class
    public KDiscipline SelectedDiscipline { get; set; }
    public KDayOfWeek SelectedDayOfWeek_NewClass { get; set; }
    public bool SelectedIsEvenWeek { get; set; }
    public TimeSpan SelectedTimeOfClass { get; set; } = new TimeSpan(hours:8, minutes:0, seconds:0);
    
    // Schedule filter
    public KDayOfWeek SelectedDayOfWeek_Filter { get; set; }
    
    public bool IsEvenSwitch { get; set; }
    public ScheduleListEditPage()
    {
        InitializeComponent();
        db = new();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Task.WaitAll(Task.Run(LoadSchedule), Task.Run(LoadDisciplines));

        var dayOfWeeks = Enum.GetValues<KDayOfWeek>();
        DayOfWeekPicker.ItemsSource = dayOfWeeks;
        
        // Add buttons to show schedule per dayOfWeek
        foreach (var el in dayOfWeeks)
        {
            VerticalStackLayout verticalStackLayout = new();
            verticalStackLayout.HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false);
            RadioButton radioButton = new RadioButton
            {
                GroupName = "DayOfWeekGroup",
                Value = el,
                HorizontalOptions = LayoutOptions.Center
            };
            radioButton.CheckedChanged += DayOfWeekSchedule_OnChanged;
            verticalStackLayout.Add(radioButton);
            verticalStackLayout.Add(new Label
            {
                Text = el.ToString()[..3],
                HorizontalTextAlignment = TextAlignment.Start
            });
            DayOfWeekStack.Add(verticalStackLayout);
        }

        var mondayLayout = DayOfWeekStack[0] as VerticalStackLayout;
        var rbMonday = mondayLayout?.Children.OfType<RadioButton>().FirstOrDefault();
        if (rbMonday != null)
            rbMonday.IsChecked = true;
    }

    private async Task LoadDisciplines()
    {
        var disciplines = (await db.Disciplines.GetDisciplines())
            .OrderBy(a => a.Name);
        Disciplines.Clear();
        foreach (var el in disciplines)
        {
            Disciplines.Add(el);
        }
    }
    
    private async Task LoadSchedule()
    {
        var schedule = await db.Schedule.GetSchedule();
        _displayedSchedule.Clear();
        foreach (var el in schedule)
        {
            _displayedSchedule.Add(el);
        }
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    //todo add display name at DisplayedSchedule
    private async Task UpdateSchedule()
    {
        DisplayedSchedule = (await db.Schedule.GetScheduleForDay(SelectedDayOfWeek_Filter))
            .Where(a => a.IsEvenWeek == IsEvenSwitch)
            .OrderBy(a => a.TimeOfClass)
            .Join(Disciplines, a => a.DisciplineId, b => b.Id,
                (schedule, discipline) =>
                {
                    schedule.DisciplineName = discipline.Name;
                    return schedule;
                })
            .ToList();
    }
    
    
    
    private async void DayOfWeekSchedule_OnChanged(object? sender, EventArgs e)
    {
        var rb = sender as RadioButton;
        SelectedDayOfWeek_Filter = (KDayOfWeek)rb.Value;
        await UpdateSchedule();
    }
    
    private async void IsEvenWeekSwitch_OnToggled(object? sender, ToggledEventArgs e)
    {
        var isEvenWeek = sender as Switch;
        IsEvenSwitch = isEvenWeek!.IsToggled;
        await UpdateSchedule();
    }
    private async void ButtonNewScheduleClass_OnClicked(object? sender, EventArgs e)
    {
        var actualSchedule = await db.Schedule.GetSchedule();
        var scheduleItem = new KSchedule
        {
            DisciplineId = SelectedDiscipline.Id,
            DayOfWeek = SelectedDayOfWeek_NewClass,
            IsEvenWeek = SelectedIsEvenWeek,
            TimeOfClass = TimeOnly.FromTimeSpan(SelectedTimeOfClass)
        };
        if (actualSchedule.Any(s =>
                   s.IsEvenWeek == scheduleItem.IsEvenWeek &&
                   s.DayOfWeek == scheduleItem.DayOfWeek &&
                   s.TimeOfClassTicks == scheduleItem.TimeOfClassTicks &&
                   s.DisciplineId == scheduleItem.DisciplineId))
            await DisplayAlert("Невозможно", "Данное занятие уже существует.", "Ок");
        else
        {
            await db.Schedule.InsertSchedule(scheduleItem);
            await UpdateSchedule();
        }
    }


    private async void ButtonDeleteClassFromSchedule_OnClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var classScheduleItem = button?.BindingContext as KSchedule;
        if (classScheduleItem == null) return;
        await db.Schedule.DeleteSchedule(classScheduleItem);
        await UpdateSchedule();
    }
}