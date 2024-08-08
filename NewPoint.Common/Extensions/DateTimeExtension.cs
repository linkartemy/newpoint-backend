using Google.Protobuf.WellKnownTypes;

namespace NewPoint.Common.Extensions;

public static class DateTimeExtension
{
    public static NullableTimestamp ToNullableTimestamp(this Timestamp? data)
    {
        return data is null
            ? new NullableTimestamp { Null = new NullValue() }
            : new NullableTimestamp { Data = data };
    }

    public static Timestamp? FromNullableTimestamp(this NullableTimestamp data)
    {
        if (data.KindCase == NullableTimestamp.KindOneofCase.Null)
        {
            return null;
        }
        return data.Data;
    }
}