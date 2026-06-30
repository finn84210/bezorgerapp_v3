namespace bezorgerapp_v3.Models;

public class AdminDeliveryOrderDto
{
    public int Id { get; set; }

    public string Status { get; set; } = string.Empty;

    public AdminCustomerDto Customer { get; set; } = new();

    public List<AdminProductDto> Products { get; set; } = new();
}

public class AdminCustomerDto
{
    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}

public class AdminProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
