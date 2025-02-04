namespace NewPoint.Models;

public class CodeData
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public int LifeTime { get; set; }
}