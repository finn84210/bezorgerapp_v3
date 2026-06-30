namespace bezorgerapp_v3.Models;

public class DeliveryOrder
{
    public int Id { get; set; }

    public int? AdminOrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = DeliveryStatus.ToDeliver;

    public string DeliveryOption { get; set; } = DeliveryOptions.HandedToCustomer;

    public string DeliveryNote { get; set; } = string.Empty;

    public DateTime? DeliveredAt { get; set; }

    public int BusNumber { get; set; }

    public List<DeliveryPackage> Packages { get; set; } = new();
}
