using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;

namespace WorkTracking.UI.ViewModels;

public class ClientStatusRowViewModel(Client client, decimal uninvoicedHours, DateOnly today)
{
    public Client Client => client;
    public string Name => client.Name;
    public decimal UninvoicedHours => uninvoicedHours;
    public decimal UninvoicedAmount => uninvoicedHours * client.HourlyRate;

    public bool HasCap => client.InvoiceCapAmount is > 0;

    public double CapProgress =>
        HasCap ? (double)Math.Min(1m, UninvoicedAmount / client.InvoiceCapAmount!.Value) : 0d;

    public bool IsOverCap =>
        InvoiceCapCalculator.Calculate(client.InvoiceCapAmount, UninvoicedAmount) == InvoiceCapStatus.OverCap;

    public bool HasFrequency => client.NextInvoiceDueDate.HasValue;

    public string NextDueLabel
    {
        get
        {
            if (client.NextInvoiceDueDate is null) return string.Empty;
            var daysUntil = client.NextInvoiceDueDate.Value.DayNumber - today.DayNumber;
            return daysUntil switch
            {
                < 0  => "Overdue",
                0    => "Due today",
                1    => "Due tomorrow",
                _    => $"Due in {daysUntil} days"
            };
        }
    }

    public bool IsOverdue =>
        InvoiceFrequencyCalculator.CalculateStatus(client.NextInvoiceDueDate, today)
            == InvoiceFrequencyStatus.Overdue;
}
