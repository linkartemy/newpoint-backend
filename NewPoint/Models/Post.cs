using Newtonsoft.Json;

namespace NewPoint.Models;

public class Post
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("author_id")]
    public long AuthorId { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
    [JsonProperty("images")]
    public string Images { get; set; }
    [JsonProperty("creation_timestamp")]
    public DateTime CreationTimeStamp { get; set; }
}