using System.Text;
using KapyTask.Database;
using KapyTask.Database.Tables;

namespace KapyTask.Views;

public partial class MainPage : ContentPage
{
    private KapyTaskDatabase db;
    
    public MainPage(KapyTaskDatabase db)
    {
        InitializeComponent();
        this.db = db;
    }

    private async void OnSetTasksClicked(object? sender, EventArgs e)
    {
        // var namesOftasks = (await db.GetTasks()).Select(a => a.Name).ToList();
        // StringBuilder sb = new();
        // foreach (var el in namesOftasks)
        // {
        //     sb.Append(el + ", ");
        // }
        //
        // Body.Text = sb.ToString();
        var el = await db.Schedule.GetSchedule();
        var count = el.Count;
        Body.Text = el.Count == 0 ? "None" : "Heeey! There it is!";
    }

}