using bezorgerapp_v3.Services;

namespace bezorgerapp_v3.Pages;

[QueryProperty(nameof(BusNumber), "busNumber")]
public partial class BusPage : ContentPage
{
    private readonly IDeliveryService _deliveryService;

    public string BusNumber { get; set; } = string.Empty;

    public BusPage(IDeliveryService deliveryService)
    {
        InitializeComponent();
        _deliveryService = deliveryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!int.TryParse(BusNumber, out var busNumber))
        {
            return;
        }

        var bus = await _deliveryService.GetBusAsync(busNumber);

        if (bus == null)
        {
            await DisplayAlertAsync("Geen bus", "Deze bus kon niet gevonden worden.", "Ok");
            return;
        }

        BusLabel.Text = $"Bus {bus.Number}";
        BusInfoLabel.Text = $"{bus.Packages.Count} pakketten verdeeld over {bus.Orders.Count} bestellingen.";
    }

    private async void OnCheckPackagesClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"packages?busNumber={BusNumber}");
    }
}
