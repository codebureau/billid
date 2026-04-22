using WorkTracking.Core.Enums;

namespace WorkTracking.Core.Services;

public static class InvoiceFrequencyCalculator
{
    /// <summary>
    /// Calculates the next invoice due date given the last invoice date and frequency in days.
    /// Returns null if no frequency is configured.
    /// </summary>
    public static DateOnly? CalculateNextDueDate(DateOnly? lastInvoiceDate, int? frequencyDays)
    {
        if (lastInvoiceDate is null || frequencyDays is null or <= 0)
            return null;

        return lastInvoiceDate.Value.AddDays(frequencyDays.Value);
    }

    /// <summary>
    /// Determines the invoice frequency status relative to today.
    /// </summary>
    public static InvoiceFrequencyStatus CalculateStatus(DateOnly? nextDueDate, DateOnly today)
    {
        if (nextDueDate is null)
            return InvoiceFrequencyStatus.NoFrequency;

        if (today < nextDueDate.Value)
            return InvoiceFrequencyStatus.OnTrack;

        if (today == nextDueDate.Value)
            return InvoiceFrequencyStatus.Due;

        return InvoiceFrequencyStatus.Overdue;
    }
}
