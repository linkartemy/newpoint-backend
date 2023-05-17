using Dapper;
using NewPoint.Handlers;
using NewPoint.Models;

namespace NewPoint.Repositories;

public class CodeRepository : ICodeRepository
{
    private static int _count;
    private readonly List<CodeData> _codes = new();
    
    public async Task AddEmailCode(int userId, string email, string code, int lifeTime)
    {
        var codeData = new CodeData
        {
            Id = _count,
            UserId = userId,
            Email = email,
            Code = code,
            LifeTime = lifeTime
        };
        var timer = new Timer(RemoveCode, _count, 0, lifeTime);
        ++_count;
        _codes.Add(codeData);
    }

    public async Task<bool> VerifyEmailCode(int userId, string email, string code)
    {
        return _codes.Count(c => c.Code == code && c.UserId == userId && c.Email == email) != 0;
    }

    private void RemoveCode(object? id)
    {
        _codes.RemoveAt(_codes.FindIndex(c => c.Id == (int)id));
    }
}