namespace bezorgerapp_v3.Services;

public class DeliveryWorkdayState
{
    public string DriverName { get; private set; } = string.Empty;

    public int? ActiveBusNumber { get; set; }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(DriverName);

    public void Login(string driverName)
    {
        DriverName = driverName.Trim();
    }

    public void Logout()
    {
        DriverName = string.Empty;
        ActiveBusNumber = null;
    }
}
