namespace NewPoint.Repositories;

public interface ICodeRepository
{
    Task AddEmailCode(int userId, string email, string code, int lifeTime = 120);
    Task<bool> VerifyEmailCode(int userId, string email, string code);
}