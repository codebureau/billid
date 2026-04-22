using FluentAssertions;
using WorkTracking.Core.Enums;
using WorkTracking.Core.Services;

namespace WorkTracking.Tests.Core;

public class InvoiceFrequencyCalculatorTests
{
    [Fact]
    public void CalculateNextDueDate_WithNullLastInvoiceDate_ReturnsNull()
    {
        var result = InvoiceFrequencyCalculator.CalculateNextDueDate(null, 90);

        result.Should().BeNull();
    }

    [Fact]
    public void CalculateNextDueDate_WithNullFrequency_ReturnsNull()
    {
        var result = InvoiceFrequencyCalculator.CalculateNextDueDate(new DateOnly(2025, 1, 1), null);

        result.Should().BeNull();
    }

    [Fact]
    public void CalculateNextDueDate_WithZeroFrequency_ReturnsNull()
    {
        var result = InvoiceFrequencyCalculator.CalculateNextDueDate(new DateOnly(2025, 1, 1), 0);

        result.Should().BeNull();
    }

    [Fact]
    public void CalculateNextDueDate_WithValidInputs_ReturnsLastDatePlusFrequency()
    {
        var result = InvoiceFrequencyCalculator.CalculateNextDueDate(new DateOnly(2025, 1, 1), 90);

        result.Should().Be(new DateOnly(2025, 4, 1));
    }

    [Fact]
    public void CalculateStatus_WithNullNextDueDate_ReturnsNoFrequency()
    {
        var result = InvoiceFrequencyCalculator.CalculateStatus(null, new DateOnly(2025, 6, 1));

        result.Should().Be(InvoiceFrequencyStatus.NoFrequency);
    }

    [Fact]
    public void CalculateStatus_WhenTodayBeforeDueDate_ReturnsOnTrack()
    {
        var result = InvoiceFrequencyCalculator.CalculateStatus(
            new DateOnly(2025, 6, 30),
            new DateOnly(2025, 6, 1));

        result.Should().Be(InvoiceFrequencyStatus.OnTrack);
    }

    [Fact]
    public void CalculateStatus_WhenTodayEqualsDueDate_ReturnsDue()
    {
        var due = new DateOnly(2025, 6, 30);

        var result = InvoiceFrequencyCalculator.CalculateStatus(due, due);

        result.Should().Be(InvoiceFrequencyStatus.Due);
    }

    [Fact]
    public void CalculateStatus_WhenTodayAfterDueDate_ReturnsOverdue()
    {
        var result = InvoiceFrequencyCalculator.CalculateStatus(
            new DateOnly(2025, 6, 30),
            new DateOnly(2025, 7, 15));

        result.Should().Be(InvoiceFrequencyStatus.Overdue);
    }
}
