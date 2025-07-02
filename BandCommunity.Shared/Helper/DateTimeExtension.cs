namespace BandCommunity.Shared.Helper;

public static class DateTimeExtension
{
    public static DateTime EnsureUtc(this DateTime dateTime)
    {
        //* Convert to UTC if not already in UTC
        return dateTime.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => dateTime, // Already in UTC
        };
    }
}