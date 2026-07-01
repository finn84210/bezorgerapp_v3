using bezorgerapp_v3.Data;
using bezorgerapp_v3.Models;

namespace bezorgerapp_v3.Services;

public class DeliveryService : IDeliveryService
{
    private readonly IAdminDeliveryClient _adminClient;
    private readonly List<DeliveryOrder> _orders;

    public DeliveryService(IAdminDeliveryClient adminClient)
    {
        _adminClient = adminClient;
        _orders = DeliveryTestData.CreateOrders();
    }

    public async Task<List<DeliveryOrder>> GetOrdersAsync()
    {
        try
        {
            var adminOrders = await _adminClient.GetPickedOrdersAsync();

            if (adminOrders.Any())
            {
                MergeAdminOrders(adminOrders);
            }
        }
        catch
        {
            // Als de adminpagina niet draait, blijft de app volledig klikbaar met testdata.
        }

        return _orders
            .OrderBy(order => order.BusNumber)
            .ThenBy(order => order.Id)
            .ToList();
    }

    public async Task<DeliveryBus?> StartDeliveryAsync(int orderId, string driverName)
    {
        var order = _orders.FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order == null)
        {
            return null;
        }

        if (order.AdminOrderId.HasValue)
        {
            await _adminClient.ClaimOrderAsync(order.AdminOrderId.Value, driverName);
        }

        return await GetBusAsync(order.BusNumber);
    }

    public Task<DeliveryBus?> GetBusAsync(int busNumber)
    {
        var busOrders = _orders
            .Where(order => order.BusNumber == busNumber)
            .OrderBy(order => order.Id)
            .ToList();

        if (!busOrders.Any())
        {
            return Task.FromResult<DeliveryBus?>(null);
        }

        return Task.FromResult<DeliveryBus?>(new DeliveryBus
        {
            Number = busNumber,
            Orders = busOrders
        });
    }

    public Task<bool> CheckPackagesAsync(int busNumber, List<int> checkedPackageIds)
    {
        var packages = _orders
            .Where(order => order.BusNumber == busNumber)
            .SelectMany(order => order.Packages)
            .ToList();

        foreach (var package in packages)
        {
            package.IsChecked = checkedPackageIds.Contains(package.Id);
        }

        return Task.FromResult(packages.All(package => package.IsChecked || package.HasIssue));
    }

    public Task<bool> ReportPackageIssueAsync(int packageId, string issueDescription)
    {
        var package = _orders
            .SelectMany(order => order.Packages)
            .FirstOrDefault(existingPackage => existingPackage.Id == packageId);

        if (package == null)
        {
            return Task.FromResult(false);
        }

        package.IsChecked = false;
        package.HasIssue = true;
        package.IssueDescription = issueDescription.Trim();

        return Task.FromResult(true);
    }

    public Task<bool> ReportRouteIssueAsync(int orderId, string issueDescription)
    {
        var order = _orders.FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order == null)
        {
            return Task.FromResult(false);
        }

        order.HasRouteIssue = true;
        order.RouteIssueDescription = issueDescription.Trim();

        return Task.FromResult(true);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
    {
        if (!DeliveryStatus.All.Contains(status))
        {
            return false;
        }

        var order = _orders.FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order == null)
        {
            return false;
        }

        order.Status = status;

        if (order.AdminOrderId.HasValue)
        {
            await _adminClient.UpdateStatusAsync(order.AdminOrderId.Value, status);
        }

        return true;
    }

    public Task<bool> SaveDeliveryOptionAsync(int orderId, string deliveryOption, string note)
    {
        if (!DeliveryOptions.All.Contains(deliveryOption))
        {
            return Task.FromResult(false);
        }

        var order = _orders.FirstOrDefault(existingOrder => existingOrder.Id == orderId);

        if (order == null)
        {
            return Task.FromResult(false);
        }

        order.DeliveryOption = deliveryOption;
        order.DeliveryNote = note.Trim();

        return Task.FromResult(true);
    }

    public async Task<bool> MarkAsDeliveredAsync(int orderId, string deliveryOption, string note)
    {
        var optionSaved = await SaveDeliveryOptionAsync(orderId, deliveryOption, note);

        if (!optionSaved)
        {
            return false;
        }

        var order = _orders.First(existingOrder => existingOrder.Id == orderId);
        order.DeliveredAt = DateTime.Now;

        return await UpdateOrderStatusAsync(orderId, DeliveryStatus.Delivered);
    }

    private void MergeAdminOrders(List<DeliveryOrder> adminOrders)
    {
        foreach (var adminOrder in adminOrders)
        {
            if (_orders.Any(order => order.AdminOrderId == adminOrder.AdminOrderId))
            {
                continue;
            }

            _orders.Add(adminOrder);
        }
    }
}
