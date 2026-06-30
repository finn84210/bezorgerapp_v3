using bezorgerapp_v3.Models;

namespace bezorgerapp_v3.Data;

public static class DeliveryTestData
{
    public static List<DeliveryOrder> CreateOrders()
    {
        return new List<DeliveryOrder>
        {
            new DeliveryOrder
            {
                Id = 1001,
                CustomerName = "Neo",
                Address = "123 Elm St",
                BusNumber = 3,
                Packages = new List<DeliveryPackage>
                {
                    new() { Id = 1, Code = "PK-1001-A", Description = "Nebuchadnezzar onderdelen" },
                    new() { Id = 2, Code = "PK-1001-B", Description = "Koelvloeistofpomp" }
                }
            },
            new DeliveryOrder
            {
                Id = 1002,
                CustomerName = "Trinity",
                Address = "789 Pine St",
                BusNumber = 3,
                Packages = new List<DeliveryPackage>
                {
                    new() { Id = 3, Code = "PK-1002-A", Description = "Jack-in Chair module" },
                    new() { Id = 4, Code = "PK-1002-B", Description = "Hydraulische cilinder" },
                    new() { Id = 5, Code = "PK-1002-C", Description = "M5 boutjes" }
                }
            },
            new DeliveryOrder
            {
                Id = 1003,
                CustomerName = "Morpheus",
                Address = "456 Oak St",
                BusNumber = 1,
                Packages = new List<DeliveryPackage>
                {
                    new() { Id = 6, Code = "PK-1003-A", Description = "EMP Device" }
                }
            }
        };
    }
}
