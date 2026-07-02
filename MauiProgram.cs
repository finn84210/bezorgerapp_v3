using bezorgerapp_v3.Pages;
using bezorgerapp_v3.Services;
using Microsoft.Extensions.Logging;

namespace bezorgerapp_v3;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<DeliveryWorkdayState>();
        builder.Services.AddSingleton<IDeliveryService, DeliveryService>();
        builder.Services.AddSingleton<IAdminDeliveryClient>(_ => new AdminDeliveryClient(new HttpClient()));

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<OrdersPage>();
        builder.Services.AddTransient<BusPage>();
        builder.Services.AddTransient<PackagesPage>();
        builder.Services.AddTransient<RoutePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
// laatste aanpassingen gemaakt.
