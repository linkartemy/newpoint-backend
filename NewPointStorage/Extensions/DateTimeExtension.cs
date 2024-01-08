using Google.Protobuf.WellKnownTypes;

namespace NewPointStorage.Extensions;

public static class DateTimeExtension
{
    public static NullableTimestamp ToNullableTimestamp(this Timestamp? data)
    {
        return data is null
            ? new NullableTimestamp { Null = new NullValue() }
            : new NullableTimestamp { Data = data };
    }
}