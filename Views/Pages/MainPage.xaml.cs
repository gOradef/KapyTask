using System.ComponentModel;
using System.Runtime.CompilerServices;
using KapyTask.Database;
using KapyTask.Database.Tables;
using KapyTask.Views.Modals;

namespace KapyTask.Views.Pages;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private KapyTaskDatabase db;
    private bool _isUpdating = false; // var to cancel tasks that already running

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = UpdateTasksAsync();    
    }

    private async Task UpdateTasksAsync()
    {
        if (_isUpdating)
            return;

        try
        {
            _isUpdating = true;
            var tasks = await db.Tasks.GetTasksWithDisciplineProperty();

            var sortedTasks = tasks.OrderBy(a => a.Deadline == null).ThenBy(a => a.Deadline).ToList();

            DisplayedKTasks = sortedTasks;
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating tasks: {e.Message}");
            await DisplayAlert("Ошибка", "Не удалось обновить задачи", "Ok");
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void ButtonCreateTask_Clicked(object? sender, EventArgs e)
    {
        Navigation.PushModalAsync(new TaskEditModal());
    }

    private void ListViewTasks_OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        var selectedTask = e.CurrentSelection.FirstOrDefault() as KTask;
        if (selectedTask is not null)
            Navigation.PushModalAsync(new TaskEditModal(selectedTask));
    }

    private async void ButtonArchiveItem_OnClicked(object? sender, EventArgs e)
    {
        var checkBox = sender as CheckBox;
        var task = checkBox.BindingContext as KTask;
        
        if ( await DisplayAlert("Потверждение", $"Точно удалить '{task.Name}'?", "Да", "Нет"))
        {
            db.Tasks.DeleteTask(task);
            UpdateTasksAsync();
        }
        else
        {
            checkBox.IsChecked = false;
        }
    }
}