namespace WorkTracking.Data.Helpers;

public static class DateConversion
{
    private const string DateFormat = "yyyy-MM-dd";
    private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";

    public static string ToIso8601(DateOnly date)
        => date.ToString(DateFormat);

    public static DateOnly ToDateOnly(string value)
        => DateOnly.ParseExact(value, DateFormat);

    public static string ToIso8601(DateTime dateTime)
        => dateTime.ToString(DateTimeFormat);

    public static DateTime ToDateTime(string value)
        => DateTime.Parse(value);

    public static string? ToIso8601Nullable(DateOnly? date)
        => date.HasValue ? date.Value.ToString(DateFormat) : null;

    public static DateOnly? ToDateOnlyNullable(string? value)
        => value is null ? null : DateOnly.ParseExact(value, DateFormat);
}
