using KapyTask.Database;
using KapyTask.Database.Tables;
using System.Collections.ObjectModel; // Add this

namespace KapyTask.Views;

public partial class SearchPage : ContentPage
{
    private KapyTaskDatabase db;
    public string Search_Text { get; set; }
    public ObservableCollection<KTask> Tasks { get; set; } = new();

    public SearchPage()
    {
        InitializeComponent();
        db = new KapyTaskDatabase(); // Initialize database
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTasks(); // Load tasks when page appears
    }

    private async Task LoadTasks()
    {
        var tasks = await db.Tasks.GetTasks();
        Tasks.Clear();
        foreach (var task in tasks)
        {
            Tasks.Add(task);
        }
    }

    private void Search_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        CurrentLabel.Text = Search_Text;
    }
}