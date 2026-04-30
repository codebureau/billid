using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;

namespace WorkTracking.UI.ViewModels;

public class ClientRowViewModel(Client client, decimal uninvoicedHours)
{
    public Client Client => client;
    public string Name => client.Name;
    public string? CompanyName => client.CompanyName;
    public bool IsActive => client.IsActive;

    public bool IsCapExceeded =>
        InvoiceCapCalculator.Calculate(
            client.InvoiceCapAmount,
            uninvoicedHours * client.HourlyRate)
        == InvoiceCapStatus.OverCap;

    public bool IsInvoiceOverdue =>
        InvoiceFrequencyCalculator.CalculateStatus(
            client.NextInvoiceDueDate,
            DateOnly.FromDateTime(DateTime.Today))
        == InvoiceFrequencyStatus.Overdue;
}
