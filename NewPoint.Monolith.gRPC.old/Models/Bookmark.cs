using Newtonsoft.Json;

namespace NewPoint.Models;

public class Bookmark
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("user_id")] public long UserId { get; set; }

    [JsonProperty("item_id")] public long ItemId { get; set; }

    [JsonProperty("creation_timestamp")] public DateTime CreationTimestamp { get; set; }
}