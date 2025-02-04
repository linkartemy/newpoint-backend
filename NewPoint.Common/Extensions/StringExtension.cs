namespace NewPoint.Common.Extensions;

public static class StringExtension
{
    public static NullableString ToNullableString(this string? data)
    {
        return data is null
            ? new NullableString { Null = new Google.Protobuf.WellKnownTypes.NullValue() }
            : new NullableString { Data = data };
    }

    public static string? FromNullableString(this NullableString data)
    {
        if (data.KindCase == NullableString.KindOneofCase.Null)
        {
            return null;
        }
        return data.Data;
    }
}