using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
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
    private List<KDiscipline> _disciplines;
    private KDiscipline _selectedDiscipline;

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

    public KDiscipline SelectedDiscipline
    {
        get => _selectedDiscipline;
        set
        {
            _selectedDiscipline = value;
            OnPropertyChanged();
            
            // Когда выбирается дисциплина, обновляем KTask
            if (KTask != null && value != null)
            {
                KTask.DisciplineId = value.Id;
                KTask.Discipline = value; // если нужно
            }
        }
    }

    public List<KDiscipline> Disciplines
    {
        get => _disciplines;
        set
        {
            _disciplines = value;
            OnPropertyChanged();
            
            // После загрузки дисциплин устанавливаем выбранную
            if (KTask != null && value != null)
            {
                SelectedDiscipline = value.FirstOrDefault(d => d.Id == KTask.DisciplineId);
            }
        }
    }
    
    public TaskEditModal(KTask? givenKTask = null)
    {
        InitializeComponent();
        db = new();
        BindingContext = this;

        
        Task.Run(async () => await LoadDisciplines());
        
        if (givenKTask is null)
            LoadPresetForNewTask();
        else
            KTask = givenKTask;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine($"Disciplines loaded: {Disciplines?.Count}");
        Debug.WriteLine($"Selected discipline: {SelectedDiscipline?.Name}");
    }

    private async Task LoadDisciplines()
    {
        var disciplines = await db.Disciplines.GetDisciplines();
        
        // Обновляем UI в главном потоке
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Disciplines = disciplines;
        });
    }
    
    private void LoadPresetForNewTask()
    {
        KTask = new KTask();
    }

    /// <summary>
    /// Dumps current task to database
    /// </summary>
    private async void ButtonConfirm_OnClicked(object? sender, EventArgs e)
    {
        if (await DisplayAlert("Подтверждение", "Вы уверены?", "Да", "Вернуться"))
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
}