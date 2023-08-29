using System.Timers;
using NewPoint.Models;
using Timer = System.Timers.Timer;

namespace NewPoint.Repositories;

public class CodeRepository : ICodeRepository
{
    private static int _count;
    private static List<CodeData> _codes = new();
    private static List<Timer> _timers = new();

    public async Task AddEmailCode(string email, string code, int lifeTime = 120000)
    {
        var codeData = new CodeData
        {
            Id = _count,
            Email = email,
            Code = code,
            LifeTime = lifeTime
        };
        var timer = new Timer(lifeTime);
        timer.Elapsed += (source, args) =>
        {
            RemoveCode(source, args, _count);
            _timers.Remove(timer);
            timer.Close();
            timer.Dispose();
        };
        timer.Start();
        _timers.Add(timer);
        ++_count;
        _codes.Add(codeData);
    }

    public async Task<bool> VerifyEmailCode(string email, string code)
    {
        return _codes.Count(c => c.Code == code && c.Email == email) != 0;
    }

    private void RemoveCode(object? source, ElapsedEventArgs args, long id)
    {
        var index = _codes.FindIndex(c => c.Id == id);
        if (index != -1) _codes.RemoveAt(index);
    }
}

public interface ICodeRepository
{
    Task AddEmailCode(string email, string code, int lifeTime = 120);
    Task<bool> VerifyEmailCode(string email, string code);
}