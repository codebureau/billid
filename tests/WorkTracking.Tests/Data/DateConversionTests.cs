using FluentAssertions;
using WorkTracking.Data.Helpers;

namespace WorkTracking.Tests.Data;

public class DateConversionTests
{
    [Fact]
    public void ToIso8601_DateOnly_FormatsCorrectly()
    {
        var date = new DateOnly(2025, 4, 7);

        DateConversion.ToIso8601(date).Should().Be("2025-04-07");
    }

    [Fact]
    public void ToDateOnly_ValidString_ParsesCorrectly()
    {
        DateConversion.ToDateOnly("2025-04-07").Should().Be(new DateOnly(2025, 4, 7));
    }

    [Fact]
    public void DateOnly_RoundTrip_PreservesValue()
    {
        var original = new DateOnly(2025, 12, 31);

        var result = DateConversion.ToDateOnly(DateConversion.ToIso8601(original));

        result.Should().Be(original);
    }

    [Fact]
    public void ToIso8601Nullable_WithNull_ReturnsNull()
    {
        DateConversion.ToIso8601Nullable(null).Should().BeNull();
    }

    [Fact]
    public void ToIso8601Nullable_WithValue_FormatsCorrectly()
    {
        DateConversion.ToIso8601Nullable(new DateOnly(2025, 1, 15)).Should().Be("2025-01-15");
    }

    [Fact]
    public void ToDateOnlyNullable_WithNull_ReturnsNull()
    {
        DateConversion.ToDateOnlyNullable(null).Should().BeNull();
    }

    [Fact]
    public void ToDateOnlyNullable_WithValue_ParsesCorrectly()
    {
        DateConversion.ToDateOnlyNullable("2025-06-30").Should().Be(new DateOnly(2025, 6, 30));
    }

    [Fact]
    public void ToIso8601_DateTime_FormatsCorrectly()
    {
        var dt = new DateTime(2025, 4, 7, 13, 30, 0);

        DateConversion.ToIso8601(dt).Should().Be("2025-04-07T13:30:00");
    }
}
