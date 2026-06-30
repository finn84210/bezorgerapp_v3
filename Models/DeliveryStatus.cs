namespace bezorgerapp_v3.Models;

public static class DeliveryStatus
{
    public const string ToDeliver = "Nog te leveren";
    public const string OnTheWay = "Onderweg";
    public const string Delivered = "Geleverd";

    public static readonly string[] All =
    [
        ToDeliver,
        OnTheWay,
        Delivered
    ];
}
