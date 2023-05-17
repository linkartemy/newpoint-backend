using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NewPoint.Configurations;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class CodeService : ICodeService
{
    private readonly ICodeRepository _codeRepository;
    private IOptions<SMTPConfiguration> _SMTPConfiguration;

    public CodeService(ICodeRepository codeRepository)
    {
        _codeRepository = codeRepository;
    }
    
    public async Task SendCodeToEmail(int userId, string email)
    {
        var code = GenerateCode();
        await _codeRepository.AddEmailCode(userId, email, code);
        
        var message = new MailMessage(_SMTPConfiguration.Value.Email, email);
        message.Subject = "Email verification code (NewPoint)";
        message.Body = @$"Code: {code}";
        var client = new SmtpClient(_SMTPConfiguration.Value.Server)
        {
            Credentials = new NetworkCredential(_SMTPConfiguration.Value.Email, _SMTPConfiguration.Value.Password),
        };
        client.Send(message);
    }

    private string GenerateCode()
        => new Random().Next(1000, 9999).ToString();
}