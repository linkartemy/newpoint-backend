using Newtonsoft.Json;

namespace NewPoint.Models;

public class User
{
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("login")]
    public string Login { get; set; } = string.Empty;
    [JsonIgnore]
    public string HashedPassword { get; set; } = string.Empty;
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    [JsonProperty("surname")]
    public string Surname { get; set; } = string.Empty;
    [JsonProperty("image")]
    public string Image { get; set; } = string.Empty;
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    [JsonProperty("phone")]
    public string Phone { get; set; } = string.Empty;
    [JsonProperty("dateOfBirth")]
    public DateTime BirthDate { get; set; }
    [JsonProperty("lastLoginTimeStamp")]
    public DateTime LastLoginTimeStamp { get; set; }
    [JsonProperty("ip")]
    public string IP { get; set; } = string.Empty;
    [JsonProperty("token")]
    public string Token { get; set; } = string.Empty;
    [JsonProperty("registrationTimeStamp")]
    public DateTime RegistrationTimeStamp { get; set; }
}
