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
            IsEnabled = !package.HasIssue,
            VerticalOptions = LayoutOptions.Center
        };
        checkBox.CheckedChanged += OnPackageCheckedChanged;
        _packageChecks.Add(checkBox);

        var labels = new VerticalStackLayout { Spacing = 2 };
        labels.Add(new Label { Text = package.Code, Style = (Style)Application.Current!.Resources["StrongText"] });
        labels.Add(new Label { Text = package.Description, Style = (Style)Application.Current.Resources["MutedText"] });

        if (package.HasIssue)
        {
            labels.Add(new Label
            {
                Text = $"Fout gemeld: {package.IssueDescription}",
                Style = (Style)Application.Current.Resources["ErrorText"]
            });
        }

        var issueButton = new Button
        {
            Text = package.HasIssue ? "Fout aanpassen" : "Fout melden",
            CommandParameter = package.Id
        };
        issueButton.Clicked += OnReportIssueClicked;

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 44 },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Padding = new Thickness(4)
        };

        grid.Add(checkBox, 0, 0);
        grid.Add(labels, 1, 0);
        grid.Add(issueButton, 2, 0);

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
        var issueCount = _bus?.Packages.Count(package => package.HasIssue) ?? 0;
        var handledCount = checkedCount + issueCount;

        ProgressLabel.Text = $"{handledCount} van {total} pakketten afgehandeld ({checkedCount} gecontroleerd, {issueCount} fout gemeld)";
        PackageProgress.Progress = total == 0 ? 0 : (double)handledCount / total;
        WarningLabel.IsVisible = false;
    }

    private async void OnReportIssueClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not int packageId)
        {
            return;
        }

        var issue = await DisplayActionSheetAsync(
            "Welke fout wil je melden?",
            "Annuleren",
            null,
            "Pakket ontbreekt",
            "Pakket beschadigd",
            "Verkeerd pakket in bus",
            "Barcode klopt niet");

        if (string.IsNullOrWhiteSpace(issue) || issue == "Annuleren")
        {
            return;
        }

        var saved = await _deliveryService.ReportPackageIssueAsync(packageId, issue);

        if (!saved)
        {
            await DisplayAlertAsync("Niet gelukt", "De foutmelding kon niet opgeslagen worden.", "Ok");
            return;
        }

        await DisplayAlertAsync("Fout gemeld", "De fout is opgeslagen bij dit pakket.", "Ok");
        await LoadPackagesAsync();
    }

    private async void OnStartRouteClicked(object? sender, EventArgs e)
    {
        if (_bus == null)
        {
            return;
        }

        var checkedIds = _bus.Packages
            .Where((package, index) => _packageChecks[index].IsChecked && !package.HasIssue)
            .Select(package => package.Id)
            .ToList();

        var allChecked = await _deliveryService.CheckPackagesAsync(_bus.Number, checkedIds);

        if (!allChecked)
        {
            WarningLabel.Text = "Controleer alle pakketten of meld een fout voordat je de route start.";
            WarningLabel.IsVisible = true;
            return;
        }

        await Shell.Current.GoToAsync($"route?busNumber={_bus.Number}");
    }
}
