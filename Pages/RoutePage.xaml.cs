using bezorgerapp_v3.Models;
using bezorgerapp_v3.Services;

namespace bezorgerapp_v3.Pages;

[QueryProperty(nameof(BusNumber), "busNumber")]
public partial class RoutePage : ContentPage
{
    private readonly IDeliveryService _deliveryService;

    public string BusNumber { get; set; } = string.Empty;

    public RoutePage(IDeliveryService deliveryService)
    {
        InitializeComponent();
        _deliveryService = deliveryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRouteAsync();
    }

    private async Task LoadRouteAsync()
    {
        if (!int.TryParse(BusNumber, out var busNumber))
        {
            return;
        }

        var bus = await _deliveryService.GetBusAsync(busNumber);

        if (bus == null)
        {
            await DisplayAlertAsync("Geen route", "Deze route kon niet gevonden worden.", "Ok");
            return;
        }

        RouteInfoLabel.Text = $"Bus {bus.Number} - {bus.Orders.Count} bestellingen";
        RouteList.Clear();

        foreach (var order in bus.Orders)
        {
            RouteList.Add(CreateRouteCard(order));
        }
    }

    private View CreateRouteCard(DeliveryOrder order)
    {
        var picker = new Picker
        {
            Title = "Status",
            ItemsSource = DeliveryStatus.All.ToList(),
            SelectedItem = order.Status
        };
        picker.SelectedIndexChanged += async (_, _) =>
        {
            if (picker.SelectedItem is string status)
            {
                await _deliveryService.UpdateOrderStatusAsync(order.Id, status);
            }
        };

        var layout = new VerticalStackLayout { Spacing = 10 };
        layout.Add(new Label { Text = $"Bestelling #{order.Id}", Style = (Style)Application.Current!.Resources["CardTitle"] });
        layout.Add(new Label { Text = order.CustomerName, Style = (Style)Application.Current.Resources["StrongText"] });
        layout.Add(new Label { Text = order.Address, Style = (Style)Application.Current.Resources["MutedText"] });
        layout.Add(new Label { Text = $"{order.Packages.Count} pakketten", Style = (Style)Application.Current.Resources["MutedText"] });
        layout.Add(picker);

        return new Border
        {
            Style = (Style)Application.Current.Resources["AppCard"],
            Content = layout
        };
    }
}
