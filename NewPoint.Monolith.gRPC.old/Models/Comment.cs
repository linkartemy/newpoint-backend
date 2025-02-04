using Newtonsoft.Json;

namespace NewPoint.Models;

public class Comment
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("user_id")] public long UserId { get; set; }

    [JsonProperty("post_id")] public long PostId { get; set; }

    [JsonProperty("likes")] public int Likes { get; set; }

    [JsonProperty("login")] public string Login { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("surname")] public string Surname { get; set; }

    [JsonProperty("content")] public string Content { get; set; }

    [JsonProperty("liked")] public bool Liked { get; set; }

    [JsonProperty("creation_timestamp")] public DateTime CreationTimestamp { get; set; }
}