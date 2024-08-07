namespace NewPoint.Common.Extensions;

public static class StringExtension
{
    public static NullableString ToNullableString(this string? data)
    {
        return data is null
            ? new NullableString { Null = new Google.Protobuf.WellKnownTypes.NullValue() }
            : new NullableString { Data = data };
    }
}