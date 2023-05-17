namespace NewPoint.Repositories;

public interface ICodeRepository
{
    Task AddEmailCode(long userId, string email, string code, int lifeTime = 120);
    Task<bool> VerifyEmailCode(long userId, string email, string code);
}