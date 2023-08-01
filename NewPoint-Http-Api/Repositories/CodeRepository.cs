using System.Timers;
using NewPoint.Models;
using Timer = System.Timers.Timer;

namespace NewPoint.Repositories;

public class CodeRepository : ICodeRepository
{
    private static int _count;
    private readonly List<CodeData> _codes = new();
    
    public async Task AddEmailCode(long userId, string email, string code, int lifeTime = 120000)
    {
        var codeData = new CodeData
        {
            Id = _count,
            UserId = userId,
            Email = email,
            Code = code,
            LifeTime = lifeTime
        };
        var timer = new Timer(lifeTime);
        timer.Elapsed += (source, args) => RemoveCode(source, args, _count);
        timer.Start();
        ++_count;
        _codes.Add(codeData);
    }

    public async Task<bool> VerifyEmailCode(long userId, string email, string code)
    {
        return _codes.Count(c => c.Code == code && c.UserId == userId && c.Email == email) != 0;
    }

    private void RemoveCode(object? source, ElapsedEventArgs args, long id)
    {
        var index = _codes.FindIndex(c => c.Id == id);
        if (index != -1)
        {
            _codes.RemoveAt(index);
        }
    }
}

public interface ICodeRepository
{
    Task AddEmailCode(long userId, string email, string code, int lifeTime = 120);
    Task<bool> VerifyEmailCode(long userId, string email, string code);
}