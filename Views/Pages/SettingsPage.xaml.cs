using KapyTask.Database;
using KapyTask.Database.Tables;
using KapyTask.Views.Modals;

namespace KapyTask.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        
    }
    
    private async void ButtonEditDisciplines_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new DisciplineEditModal());
    }

    private async void ButtonEditSchedule_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new ScheduleEditModal());
    }

    private async void ButtonOpenDevPage_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutPage());
    }
}