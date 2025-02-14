using Newtonsoft.Json;

namespace NewPoint.Common.Models;

public class User
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("login")] public string Login { get; set; } = "Unknown";

    [JsonIgnore] public string HashedPassword { get; set; } = string.Empty;

    [JsonProperty("name")] public string Name { get; set; } = "Unknown";

    [JsonProperty("surname")] public string Surname { get; set; } = string.Empty;

    [JsonProperty("description")] public string? Description { get; set; }

    [JsonProperty("location")] public string? Location { get; set; }

    [JsonProperty("email")] public string? Email { get; set; }

    [JsonProperty("phone")] public string? Phone { get; set; }

    [JsonProperty("profileImageId")] public long ProfileImageId { get; set; } = 0;

    [JsonProperty("headerImageId")] public long HeaderImageId { get; set; }

    [JsonProperty("birthDate")] public DateTime BirthDate { get; set; }

    [JsonProperty("registrationTimestamp")]
    public DateTime RegistrationTimestamp { get; set; }

    [JsonProperty("lastLoginTimestamp")] public DateTime LastLoginTimestamp { get; set; }

    [JsonProperty("ip")] public string? IP { get; set; }
    [JsonProperty("followers")] public int Followers { get; set; }
    [JsonProperty("following")] public int Following { get; set; }
}