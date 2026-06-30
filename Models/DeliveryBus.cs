namespace bezorgerapp_v3.Models;

public class DeliveryBus
{
    public int Number { get; set; }

    public string DriverName { get; set; } = string.Empty;

    public List<DeliveryOrder> Orders { get; set; } = new();

    public List<DeliveryPackage> Packages => Orders.SelectMany(order => order.Packages).ToList();
}
