namespace NewPoint.Services;

public interface ICodeService
{
    Task SendCodeToEmail(int userId, string email);
}