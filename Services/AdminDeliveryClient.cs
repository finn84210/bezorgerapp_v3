using System.Net.Http.Json;
using bezorgerapp_v3.Models;

namespace bezorgerapp_v3.Services;

public class AdminDeliveryClient : IAdminDeliveryClient
{
    private const string AdminApiBaseUrl = "http://localhost:5177";
    private readonly HttpClient _httpClient;

    public AdminDeliveryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(AdminApiBaseUrl);
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

    public Task UpdateStatusAsync(int adminOrderId, string status)
    {
        // De adminpagina heeft nu nog geen los endpoint voor statusupdates vanuit MAUI.
        // Door deze methode alvast te maken, hoeft later alleen deze client aangepast te worden.
        return Task.CompletedTask;
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
