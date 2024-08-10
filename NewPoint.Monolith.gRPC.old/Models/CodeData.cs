namespace NewPoint.Models;

public enum CodeType
{
    EmailVerification,
    PhoneVerification,
    PasswordVerification
}

public class CodeData
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Code { get; set; }
    public CodeType CodeType { get; set; }
    public int LifeTime { get; set; } = 120000;
}