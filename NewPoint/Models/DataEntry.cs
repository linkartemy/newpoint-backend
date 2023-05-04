using Newtonsoft.Json;

namespace NewPoint.Models;

public class DataEntry<T> : IDataEntry
{
    [JsonProperty("id")]
    public ulong Id { get; set; }
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;
    [JsonProperty("data")]
    public T Data { get; set; }
}
