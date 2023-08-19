using Google.Protobuf.WellKnownTypes;

namespace NewPoint.Handlers;

public static class DateTimeHandler
{
    public static bool TryTimestampToDateTime(Timestamp timestamp, out DateTime dateTime)
    {
        try
        {
            dateTime = timestamp.ToDateTime();
            return true;
        }
        catch (Exception)
        {
            dateTime = DateTime.Now;
            return false;
        }
    }

    public static DateTime? TimestampToDateTime(Timestamp? timestamp)
    {
        if (timestamp is null) return null;
        return timestamp.ToDateTime();
    }

    public static bool TryTimestampToDateTime(DateTime dateTime, out Timestamp timestamp)
    {
        try
        {
            timestamp = dateTime.ToUniversalTime().ToTimestamp();
            return true;
        }
        catch (Exception)
        {
            timestamp = Timestamp.FromDateTime(DateTime.Now);
            return false;
        }
    }

    public static Timestamp DateTimeToTimestamp(DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToTimestamp();
    }
}