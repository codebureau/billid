using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;

namespace WorkTracking.Core.Services;

public static class InvoiceCapCalculator
{
    /// <summary>
    /// Calculates the cap status for a set of uninvoiced work entries against a client's cap.
    /// </summary>
    public static InvoiceCapStatus Calculate(decimal? capAmount, decimal totalAmount)
    {
        if (capAmount is null or <= 0)
            return InvoiceCapStatus.NoCap;

        if (totalAmount < capAmount.Value)
            return InvoiceCapStatus.UnderCap;

        if (totalAmount == capAmount.Value)
            return InvoiceCapStatus.AtCap;

        return InvoiceCapStatus.OverCap;
    }

    /// <summary>
    /// Calculates the total billable amount from a set of work entries at a given hourly rate.
    /// </summary>
    public static decimal CalculateTotalAmount(IEnumerable<WorkEntry> entries, decimal hourlyRate)
        => entries.Sum(e => e.Hours) * hourlyRate;
}
