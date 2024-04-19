using Newtonsoft.Json;

namespace NewPoint.Models;

public class Post
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("author_id")] public long AuthorId { get; set; }

    [JsonProperty("login")] public string Login { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("surname")] public string Surname { get; set; }
    [JsonProperty("profile_image_id")] public long ProfileImageId { get; set; }

    [JsonProperty("content")] public string Content { get; set; }

    [JsonProperty("images")] public string Images { get; set; }

    [JsonProperty("likes")] public int Likes { get; set; }

    [JsonProperty("shares")] public int Shares { get; set; }

    [JsonProperty("comments")] public int Comments { get; set; }
    [JsonProperty("views")] public int Views { get; set; }

    [JsonProperty("liked")] public bool Liked { get; set; }

    [JsonProperty("bookmarked")] public bool Bookmarked { get; set; }

    [JsonProperty("creation_timestamp")] public DateTime CreationTimestamp { get; set; }
}