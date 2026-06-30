using bezorgerapp_v3.Models;

namespace bezorgerapp_v3.Services;

public interface IAdminDeliveryClient
{
    Task<List<DeliveryOrder>> GetPickedOrdersAsync();

    Task ClaimOrderAsync(int adminOrderId, string driverName);

    Task UpdateStatusAsync(int adminOrderId, string status);
}
