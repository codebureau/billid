using FluentAssertions;
using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;

namespace WorkTracking.Tests.Core;

public class InvoiceCapCalculatorTests
{
    [Fact]
    public void Calculate_WithNullCap_ReturnsNoCap()
    {
        var result = InvoiceCapCalculator.Calculate(null, 500m);

        result.Should().Be(InvoiceCapStatus.NoCap);
    }

    [Fact]
    public void Calculate_WithZeroCap_ReturnsNoCap()
    {
        var result = InvoiceCapCalculator.Calculate(0m, 500m);

        result.Should().Be(InvoiceCapStatus.NoCap);
    }

    [Fact]
    public void Calculate_WhenTotalBelowCap_ReturnsUnderCap()
    {
        var result = InvoiceCapCalculator.Calculate(1000m, 800m);

        result.Should().Be(InvoiceCapStatus.UnderCap);
    }

    [Fact]
    public void Calculate_WhenTotalEqualsCap_ReturnsAtCap()
    {
        var result = InvoiceCapCalculator.Calculate(1000m, 1000m);

        result.Should().Be(InvoiceCapStatus.AtCap);
    }

    [Fact]
    public void Calculate_WhenTotalExceedsCap_ReturnsOverCap()
    {
        var result = InvoiceCapCalculator.Calculate(1000m, 1200m);

        result.Should().Be(InvoiceCapStatus.OverCap);
    }

    [Fact]
    public void CalculateTotalAmount_WithMultipleEntries_ReturnsSumOfHoursTimesRate()
    {
        var entries = new List<WorkEntry>
        {
            new() { Hours = 2m },
            new() { Hours = 3m },
            new() { Hours = 1.5m }
        };

        var result = InvoiceCapCalculator.CalculateTotalAmount(entries, 100m);

        result.Should().Be(650m);
    }

    [Fact]
    public void CalculateTotalAmount_WithEmptyEntries_ReturnsZero()
    {
        var result = InvoiceCapCalculator.CalculateTotalAmount([], 100m);

        result.Should().Be(0m);
    }
}
