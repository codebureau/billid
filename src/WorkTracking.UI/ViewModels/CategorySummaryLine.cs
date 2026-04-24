namespace WorkTracking.UI.ViewModels;

public class CategorySummaryLine(string categoryName, decimal hours, decimal amount)
{
    public string CategoryName { get; } = categoryName;
    public decimal Hours { get; } = hours;
    public decimal Amount { get; } = amount;
}
