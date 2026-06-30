using bezorgerapp_v3.Pages;

namespace bezorgerapp_v3;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("bus", typeof(BusPage));
        Routing.RegisterRoute("packages", typeof(PackagesPage));
        Routing.RegisterRoute("route", typeof(RoutePage));
    }
}
