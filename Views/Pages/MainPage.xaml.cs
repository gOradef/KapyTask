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
        DisplayedKTasks = await db.Tasks.GetTasksWithDisciplineProperty();
    }

    private async void ButtonCreateTask_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new TaskEditModal());
    }

    public new event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void ListViewTasks_OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        await Navigation.PushModalAsync(new TaskEditModal(e.SelectedItem as KTask));
    }

    private async void ButtonArchiveItem_OnClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var task = button.BindingContext as KTask;
        
        if ( await DisplayAlert("Потверждение", $"Точно удалить '{task.Name}'?", "Да", "Нет"))
        {
            db.Tasks.DeleteTask(task);
            UpdateTasks();
        }
    }
}