using bezorgerapp_v3.Models;

namespace bezorgerapp_v3.Services;

public interface IDeliveryService
{
    Task<List<DeliveryOrder>> GetOrdersAsync();

    Task<DeliveryBus?> StartDeliveryAsync(int orderId, string driverName);

    Task<DeliveryBus?> GetBusAsync(int busNumber);

    Task<bool> CheckPackagesAsync(int busNumber, List<int> checkedPackageIds);

    Task<bool> UpdateOrderStatusAsync(int orderId, string status);
}
