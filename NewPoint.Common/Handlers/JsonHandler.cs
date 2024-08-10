using System.Data;
using Dapper;
using Newtonsoft.Json;

namespace NewPoint.Common.Handlers;

public static class JsonHandler
{
    public static T Parse<T>(object value)
    {
        return JsonConvert.DeserializeObject<T>(value as string);
    }

    public static string Stringify(object value)
    {
        return JsonConvert.SerializeObject(value);
    }
}
