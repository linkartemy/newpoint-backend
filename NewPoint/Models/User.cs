using Newtonsoft.Json;

namespace NewPoint.Models;

public class User
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("login")] public string Login { get; set; } = string.Empty;

    [JsonIgnore] public string HashedPassword { get; set; } = string.Empty;

    [JsonProperty("name")] public string Name { get; set; } = string.Empty;

    [JsonProperty("surname")] public string Surname { get; set; } = string.Empty;

    [JsonProperty("image")] public string Image { get; set; } = string.Empty;

    [JsonProperty("email")] public string Email { get; set; } = string.Empty;

    [JsonProperty("phone")] public string Phone { get; set; } = string.Empty;

    [JsonProperty("birthDate")] public DateTime BirthDate { get; set; }

    [JsonProperty("lastLoginTimestamp")] public DateTime LastLoginTimestamp { get; set; }

    [JsonProperty("ip")] public string IP { get; set; } = string.Empty;

    [JsonProperty("registrationTimestamp")]
    public DateTime RegistrationTimestamp { get; set; }
}