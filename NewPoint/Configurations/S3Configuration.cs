namespace NewPoint.Configurations;

public class S3Configuration
{
    public string EntryPoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string UserImagesBucket { get; set; }
    
    public S3Configuration(IConfigurationSection configurationSection)
    {
        EntryPoint = configurationSection.GetValue<string>("EntryPoint");
        AccessKey = configurationSection.GetValue<string>("AccessKey");
        SecretKey = configurationSection.GetValue<string>("SecretKey");
        UserImagesBucket = configurationSection.GetValue<string>("UserImagesBucket");
    }
}