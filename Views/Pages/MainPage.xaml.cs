using System.ComponentModel;
using System.Runtime.CompilerServices;
using KapyTask.Database;
using KapyTask.Database.Tables;
using KapyTask.Views.Modals;

namespace KapyTask.Views.Pages;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private KapyTaskDatabase db;

    private List<KTask> _displayedKTasks = new();
    public List<KTask> DisplayedKTasks
    {
        get => _displayedKTasks;
        set
        {
            _displayedKTasks = value;
            OnPropertyChanged();
        }
    }

    public MainPage(KapyTaskDatabase db)
    {
        InitializeComponent();
        this.db = db;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateTasks();
    }

    private async Task UpdateTasks()
    {
        DisplayedKTasks = (await db.Tasks.GetTasksWithDisciplineProperty()).OrderBy(a => a.Deadline is null)
            .ThenBy(a => a.Deadline)
            .ToList();
    }

    private async void ButtonCreateTask_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new TaskEditModal());
    }

    private async void ListViewTasks_OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        await Navigation.PushModalAsync(new TaskEditModal(e.SelectedItem as KTask));
    }

    private async void ButtonArchiveItem_OnClicked(object? sender, EventArgs e)
    {
        var button = sender as CheckBox;
        var task = button.BindingContext as KTask;
        
        if ( await DisplayAlert("Потверждение", $"Точно удалить '{task.Name}'?", "Да", "Нет"))
        {
            db.Tasks.DeleteTask(task);
            UpdateTasks();
        }
    }
}