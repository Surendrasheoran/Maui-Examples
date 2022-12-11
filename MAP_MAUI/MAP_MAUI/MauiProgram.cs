using Microsoft.Extensions.Logging;

namespace MAP_MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        builder.ConfigureMauiHandlers(handlers =>
        {
#if IOS || MACCATALYST
			handlers.AddHandler<CustomPin, CustomPinHandler>();
#endif
        });
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

