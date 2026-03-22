using System.Windows.Input;

namespace KapyTask.Views.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
    
    public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url));
}