using System.Net.Http.Json;
using bezorgerapp_v3.Models;
using Microsoft.Maui.Devices;

namespace bezorgerapp_v3.Services;

public class AdminDeliveryClient : IAdminDeliveryClient
{
    private readonly HttpClient _httpClient;

    public AdminDeliveryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(GetAdminApiBaseUrl());
    }

    private static string GetAdminApiBaseUrl()
    {
        return DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5177"
            : "http://localhost:5177";
    }

    public async Task<List<DeliveryOrder>> GetPickedOrdersAsync()
    {
        var adminOrders = await _httpClient.GetFromJsonAsync<List<AdminDeliveryOrderDto>>("/api/delivery/orders");

        return adminOrders?
            .Select((order, index) => MapAdminOrder(order, index))
            .ToList() ?? new List<DeliveryOrder>();
    }

    public async Task ClaimOrderAsync(int adminOrderId, string driverName)
    {
        await _httpClient.PostAsJsonAsync($"/api/delivery/orders/{adminOrderId}/claim", new
        {
            deliveryPerson = driverName
        });
    }

    public async Task UpdateStatusAsync(int adminOrderId, string status)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/delivery/orders/{adminOrderId}/status")
        {
            Content = JsonContent.Create(new
            {
                status
            })
        };

        await _httpClient.SendAsync(request);
    }

    private static DeliveryOrder MapAdminOrder(AdminDeliveryOrderDto order, int index)
    {
        var packages = order.Products.Any()
            ? order.Products.Select(product => new DeliveryPackage
            {
                Id = product.Id,
                Code = $"PK-{order.Id}-{product.Id}",
                Description = product.Name
            }).ToList()
            : new List<DeliveryPackage>
            {
                new()
                {
                    Id = order.Id * 10,
                    Code = $"PK-{order.Id}-1",
                    Description = "Orderpakket"
                }
            };

        return new DeliveryOrder
        {
            Id = order.Id,
            AdminOrderId = order.Id,
            CustomerName = order.Customer.Name,
            Address = order.Customer.Address,
            Status = DeliveryStatus.ToDeliver,
            BusNumber = 1 + (index % 3),
            Packages = packages
        };
    }
}
