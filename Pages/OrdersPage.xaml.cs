using bezorgerapp_v3.Models;
using bezorgerapp_v3.Services;

namespace bezorgerapp_v3.Pages;

public partial class OrdersPage : ContentPage
{
    private readonly IDeliveryService _deliveryService;
    private readonly DeliveryWorkdayState _workdayState;

    public OrdersPage(IDeliveryService deliveryService, DeliveryWorkdayState workdayState)
    {
        InitializeComponent();
        _deliveryService = deliveryService;
        _workdayState = workdayState;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadOrdersAsync();
        OrdersRefreshView.IsRefreshing = false;
    }

    private async Task LoadOrdersAsync()
    {
        var orders = await _deliveryService.GetOrdersAsync();

        IntroLabel.Text = $"Goedemorgen {_workdayState.DriverName}. Kies een bestelling om je bus te starten.";
        CountLabel.Text = $"{orders.Count} bestellingen";
        OrdersList.Clear();

        foreach (var order in orders)
        {
            OrdersList.Add(CreateOrderCard(order));
        }
    }

    private View CreateOrderCard(DeliveryOrder order)
    {
        var button = new Button
        {
            Text = "Start bezorgen",
            Style = (Style)Application.Current!.Resources["PrimaryButton"],
            CommandParameter = order.Id
        };
        button.Clicked += OnStartDeliveryClicked;

        var layout = new VerticalStackLayout { Spacing = 10 };
        layout.Add(new Label { Text = $"Bestelling #{order.Id}", Style = (Style)Application.Current!.Resources["CardTitle"] });
        layout.Add(new Label { Text = order.CustomerName, Style = (Style)Application.Current.Resources["StrongText"] });
        layout.Add(new Label { Text = order.Address, Style = (Style)Application.Current.Resources["MutedText"] });
        layout.Add(CreateOrderDetails(order));
        layout.Add(button);

        return new Border
        {
            Style = (Style)Application.Current.Resources["AppCard"],
            Content = layout
        };
    }

    private static Grid CreateOrderDetails(DeliveryOrder order)
    {
        var grid = new Grid
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
            RowSpacing = 6,
            ColumnSpacing = 12
        };

        grid.Add(new Label { Text = "Status", Style = (Style)Application.Current!.Resources["FieldLabel"] }, 0, 0);
        grid.Add(new Label { Text = "Pakketten", Style = (Style)Application.Current.Resources["FieldLabel"] }, 1, 0);
        grid.Add(new Label { Text = order.Status }, 0, 1);
        grid.Add(new Label { Text = order.Packages.Count.ToString() }, 1, 1);

        return grid;
    }

    private async void OnStartDeliveryClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not int orderId)
        {
            return;
        }

        var bus = await _deliveryService.StartDeliveryAsync(orderId, _workdayState.DriverName);

        if (bus == null)
        {
            await DisplayAlertAsync("Niet gevonden", "Deze bestelling kon niet gestart worden.", "Ok");
            return;
        }

        _workdayState.ActiveBusNumber = bus.Number;
        await Shell.Current.GoToAsync($"bus?busNumber={bus.Number}");
    }
}
