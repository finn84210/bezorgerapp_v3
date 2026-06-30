namespace bezorgerapp_v3.Components;

public static class DeliveryText
{
    public static string PackageSummary(int packageCount)
    {
        return packageCount == 1 ? "1 pakket" : $"{packageCount} pakketten";
    }
}
