using bezorgerapp_v3.Models;
using bezorgerapp_v3.Services;
using Microsoft.Maui.ApplicationModel;

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
        var statusPicker = new Picker
        {
            Title = "Status",
            ItemsSource = DeliveryStatus.All.ToList(),
            SelectedItem = order.Status
        };
        statusPicker.SelectedIndexChanged += async (_, _) =>
        {
            if (statusPicker.SelectedItem is string status)
            {
                await _deliveryService.UpdateOrderStatusAsync(order.Id, status);
            }
        };

        var deliveryOptionPicker = new Picker
        {
            Title = "Bezorgoptie",
            ItemsSource = DeliveryOptions.All.ToList(),
            SelectedItem = order.DeliveryOption
        };

        var noteEntry = new Entry
        {
            Placeholder = "Notitie, bijv. huisnummer buren",
            Text = order.DeliveryNote
        };

        var mapsButton = new Button
        {
            Text = "Open in Google Maps",
            Style = (Style)Application.Current!.Resources["PrimaryButton"]
        };
        mapsButton.Clicked += async (_, _) => await OpenGoogleMapsAsync(order.Address);

        var onTheWayButton = new Button
        {
            Text = "Zet op onderweg"
        };
        onTheWayButton.Clicked += async (_, _) =>
        {
            statusPicker.SelectedItem = DeliveryStatus.OnTheWay;
            await _deliveryService.UpdateOrderStatusAsync(order.Id, DeliveryStatus.OnTheWay);
            await DisplayAlertAsync("Status bijgewerkt", $"Bestelling #{order.Id} staat op onderweg.", "Ok");
        };

        var saveOptionButton = new Button
        {
            Text = "Bezorgoptie opslaan"
        };
        saveOptionButton.Clicked += async (_, _) =>
        {
            var option = deliveryOptionPicker.SelectedItem?.ToString() ?? DeliveryOptions.HandedToCustomer;
            await _deliveryService.SaveDeliveryOptionAsync(order.Id, option, noteEntry.Text ?? string.Empty);
            await DisplayAlertAsync("Opgeslagen", "De bezorgoptie is opgeslagen.", "Ok");
        };

        var deliveredButton = new Button
        {
            Text = "Zet op afgeleverd"
        };
        deliveredButton.Clicked += async (_, _) =>
        {
            var option = deliveryOptionPicker.SelectedItem?.ToString() ?? DeliveryOptions.HandedToCustomer;
            var delivered = await _deliveryService.MarkAsDeliveredAsync(order.Id, option, noteEntry.Text ?? string.Empty);

            if (!delivered)
            {
                await DisplayAlertAsync("Niet gelukt", "Deze bestelling kon niet op afgeleverd gezet worden.", "Ok");
                return;
            }

            statusPicker.SelectedItem = DeliveryStatus.Delivered;
            await DisplayAlertAsync("Afgeleverd", $"Bestelling #{order.Id} is afgeleverd.", "Ok");
            await LoadRouteAsync();
        };

        var actionGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Star }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnSpacing = 10,
            RowSpacing = 10
        };
        actionGrid.Add(mapsButton, 0, 0);
        actionGrid.Add(onTheWayButton, 1, 0);
        actionGrid.Add(saveOptionButton, 0, 1);
        actionGrid.Add(deliveredButton, 1, 1);

        var layout = new VerticalStackLayout { Spacing = 10 };
        layout.Add(new Label { Text = $"Bestelling #{order.Id}", Style = (Style)Application.Current!.Resources["CardTitle"] });
        layout.Add(new Label { Text = order.CustomerName, Style = (Style)Application.Current.Resources["StrongText"] });
        layout.Add(new Label { Text = order.Address, Style = (Style)Application.Current.Resources["MutedText"] });
        layout.Add(new Label { Text = $"{order.Packages.Count} pakketten", Style = (Style)Application.Current.Resources["MutedText"] });
        layout.Add(CreateDeliveredLabel(order));
        layout.Add(new Label { Text = "Status", Style = (Style)Application.Current.Resources["FieldLabel"] });
        layout.Add(statusPicker);
        layout.Add(new Label { Text = "Bezorgoptie", Style = (Style)Application.Current.Resources["FieldLabel"] });
        layout.Add(deliveryOptionPicker);
        layout.Add(noteEntry);
        layout.Add(actionGrid);

        return new Border
        {
            Style = (Style)Application.Current.Resources["AppCard"],
            Content = layout
        };
    }

    private static Label CreateDeliveredLabel(DeliveryOrder order)
    {
        var text = order.DeliveredAt.HasValue
            ? $"Afgeleverd om {order.DeliveredAt.Value:HH:mm} via: {order.DeliveryOption}"
            : "Nog niet afgeleverd";

        return new Label
        {
            Text = text,
            Style = (Style)Application.Current!.Resources["MutedText"]
        };
    }

    private static async Task OpenGoogleMapsAsync(string address)
    {
        var encodedAddress = Uri.EscapeDataString(address);
        var mapsUrl = $"https://www.google.com/maps/search/?api=1&query={encodedAddress}";

        // Launcher opent de standaard browser of Google Maps-app als die op het apparaat staat.
        await Launcher.OpenAsync(mapsUrl);
    }
}
