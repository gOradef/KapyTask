using KapyTask.Database;
using Microsoft.Extensions.Logging;
using HorusStudio.Maui.MaterialDesignControls;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;

namespace KapyTask;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitCore()
            .UseMaterialDesignControls()
            .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        }).UseMauiCommunityToolkit();
        builder.Services.AddSingleton<KapyTaskDatabase>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}