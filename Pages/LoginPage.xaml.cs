using bezorgerapp_v3.Services;

namespace bezorgerapp_v3.Pages;

public partial class LoginPage : ContentPage
{
    private readonly DeliveryWorkdayState _workdayState;

    public LoginPage(DeliveryWorkdayState workdayState)
    {
        InitializeComponent();
        _workdayState = workdayState;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        var userName = UserNameEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            ErrorLabel.Text = "Vul je gebruikersnaam en wachtwoord in.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Voor deze studentenapp is elke ingevulde login geldig.
        _workdayState.Login(userName);
        ErrorLabel.IsVisible = false;

        await Shell.Current.GoToAsync("//orders");
    }
}
