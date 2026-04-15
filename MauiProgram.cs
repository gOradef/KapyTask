using KapyTask.Database;
using Microsoft.Extensions.Logging;
using HorusStudio.Maui.MaterialDesignControls;

namespace KapyTask;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMaterialDesignControls(options =>
            {
                // options.ConfigureFontSizeFromResources();
                // options.OnException((ex) =>
                // {
                // });
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.Services.AddSingleton<KapyTaskDatabase>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}