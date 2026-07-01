namespace bezorgerapp_v3.Models;

public class DeliveryPackage
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsChecked { get; set; }

    public bool HasIssue { get; set; }

    public string IssueDescription { get; set; } = string.Empty;
}
