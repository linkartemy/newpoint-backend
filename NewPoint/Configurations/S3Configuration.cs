namespace NewPoint.Configurations;

public class S3Configuration
{
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string UserImagesBucket { get; set; }

    public S3Configuration()
    {
    }

    public S3Configuration(IConfigurationSection configurationSection)
    {
        Endpoint = configurationSection.GetValue<string>("Endpoint");
        AccessKey = configurationSection.GetValue<string>("AccessKey");
        SecretKey = configurationSection.GetValue<string>("SecretKey");
        UserImagesBucket = configurationSection.GetValue<string>("UserImagesBucket");
    }
}