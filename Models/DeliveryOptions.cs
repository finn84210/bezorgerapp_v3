namespace bezorgerapp_v3.Models;

public static class DeliveryOptions
{
    public const string HandedToCustomer = "Afgegeven aan klant";
    public const string LeftWithNeighbor = "Afgegeven bij buren";
    public const string SafePlace = "Op veilige plek gelegd";
    public const string NotHome = "Klant niet thuis";

    public static readonly string[] All =
    [
        HandedToCustomer,
        LeftWithNeighbor,
        SafePlace,
        NotHome
    ];
}
