using bezorgerapp_v3.Models;
using bezorgerapp_v3.Services;

namespace bezorgerapp_v3.Pages;

[QueryProperty(nameof(BusNumber), "busNumber")]
public partial class PackagesPage : ContentPage
{
    private readonly IDeliveryService _deliveryService;
    private readonly List<CheckBox> _packageChecks = new();
    private DeliveryBus? _bus;

    public string BusNumber { get; set; } = string.Empty;

    public PackagesPage(IDeliveryService deliveryService)
    {
        InitializeComponent();
        _deliveryService = deliveryService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPackagesAsync();
    }

    private async Task LoadPackagesAsync()
    {
        if (!int.TryParse(BusNumber, out var busNumber))
        {
            return;
        }

        _bus = await _deliveryService.GetBusAsync(busNumber);

        if (_bus == null)
        {
            await DisplayAlertAsync("Geen bus", "Deze bus kon niet gevonden worden.", "Ok");
            return;
        }

        _packageChecks.Clear();
        PackagesList.Clear();

        foreach (var package in _bus.Packages)
        {
            PackagesList.Add(CreatePackageRow(package));
        }

        UpdateProgress();
    }

    private View CreatePackageRow(DeliveryPackage package)
    {
        var checkBox = new CheckBox
        {
            IsChecked = package.IsChecked,
            VerticalOptions = LayoutOptions.Center
        };
        checkBox.CheckedChanged += OnPackageCheckedChanged;
        _packageChecks.Add(checkBox);

        var labels = new VerticalStackLayout { Spacing = 2 };
        labels.Add(new Label { Text = package.Code, Style = (Style)Application.Current!.Resources["StrongText"] });
        labels.Add(new Label { Text = package.Description, Style = (Style)Application.Current.Resources["MutedText"] });

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 44 },
                new ColumnDefinition { Width = GridLength.Star }
            },
            Padding = new Thickness(4)
        };

        grid.Add(checkBox, 0, 0);
        grid.Add(labels, 1, 0);

        return new Border
        {
            Style = (Style)Application.Current.Resources["AppCard"],
            Content = grid
        };
    }

    private void OnPackageCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        var total = _packageChecks.Count;
        var checkedCount = _packageChecks.Count(checkBox => checkBox.IsChecked);

        ProgressLabel.Text = $"{checkedCount} van {total} pakketten gecontroleerd";
        PackageProgress.Progress = total == 0 ? 0 : (double)checkedCount / total;
        WarningLabel.IsVisible = false;
    }

    private async void OnStartRouteClicked(object? sender, EventArgs e)
    {
        if (_bus == null)
        {
            return;
        }

        var checkedIds = _bus.Packages
            .Where((_, index) => _packageChecks[index].IsChecked)
            .Select(package => package.Id)
            .ToList();

        var allChecked = await _deliveryService.CheckPackagesAsync(_bus.Number, checkedIds);

        if (!allChecked)
        {
            WarningLabel.Text = "Controleer eerst alle pakketten voordat je de route start.";
            WarningLabel.IsVisible = true;
            return;
        }

        await Shell.Current.GoToAsync($"route?busNumber={_bus.Number}");
    }
}
