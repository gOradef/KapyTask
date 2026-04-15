using HorusStudio.Maui.MaterialDesignControls;

namespace KapyTask;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MaterialDesignControls.InitializeComponents();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}