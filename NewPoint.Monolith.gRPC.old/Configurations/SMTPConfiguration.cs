namespace NewPoint.Configurations;

public class SmtpConfiguration
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Server { get; set; }
    public int Port { get; set; }

    public SmtpConfiguration()
    {
    }

    public SmtpConfiguration(IConfigurationSection configurationSection)
    {
        Email = configurationSection.GetValue<string>("email");
        Password = configurationSection.GetValue<string>("password");
        Server = configurationSection.GetValue<string>("server");
        Port = configurationSection.GetValue<int>("port");
    }
}