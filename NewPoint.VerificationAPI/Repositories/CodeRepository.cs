using System.Timers;
using NewPoint.Common.Models;
using Timer = System.Timers.Timer;

namespace NewPoint.VerificationAPI.Repositories;

public class CodeRepository : ICodeRepository
{
    private static int _count;
    private static List<CodeData> _codes = new();
    private static List<Timer> _timers = new();

    public async Task AddCode(CodeData codeData)
    {
        var timer = new Timer(codeData.LifeTime);
        timer.Elapsed += (source, args) =>
        {
            RemoveCode(source, args, _count);
            _timers.Remove(timer);
            timer.Close();
            timer.Dispose();
        };
        timer.Start();
        _timers.Add(timer);
        codeData.Id = _count;
        ++_count;
        _codes.Add(codeData);
    }

    public async Task<bool> VerifyCode(CodeData codeData)
    {
        return _codes.Count(c => c.Code == codeData.Code && c.CodeType == codeData.CodeType) != 0;
    }

    private void RemoveCode(object? source, ElapsedEventArgs args, long id)
    {
        var index = _codes.FindIndex(c => c.Id == id);
        if (index != -1) _codes.RemoveAt(index);
    }
}

public interface ICodeRepository
{
    Task AddCode(CodeData codeData);
    Task<bool> VerifyCode(CodeData codeData);
}