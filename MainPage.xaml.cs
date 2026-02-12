using System.Text;
using KapyTask.Database;
using KapyTask.Database.Tables;

namespace KapyTask;

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
        var namesOftasks = (await db.GetAllTasks()).Select(a => a.Name).ToList();
        StringBuilder sb = new();
        foreach (var el in namesOftasks)
        {
            sb.Append(el + ", ");
        }

        Body.Text = sb.ToString();
    }

    private async void InsertNewTempTask_OnClicked(object? sender, EventArgs e)
    {
        var last = (await db.GetAllTasks()).LastOrDefault();
        int str = 0;
        if (last is not null)
            str = last.Id;
        var task = new KTask("TempHAHAHA: " + str + "<br>");
        await db.InsertTask(task);
    }
}