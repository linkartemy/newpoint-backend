using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NewPoint.Configurations;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class CodeService : ICodeService
{
    private ICodeRepository _codeRepository;
    private IOptions<SMTPConfiguration> _smtpConfiguration;

    public CodeService(ICodeRepository codeRepository, IOptions<SMTPConfiguration> smtpConfiguration)
    {
        _codeRepository = codeRepository;
        _smtpConfiguration = smtpConfiguration;
    }
    
    public async Task SendCodeToEmail(int userId, string email)
    {
        var code = GenerateCode();
       await _codeRepository.AddEmailCode(userId, email, code);
        
        var message = new MailMessage(_smtpConfiguration.Value.Email, email);
        message.Subject = "Email verification code (NewPoint)";
        message.Body = @$"Code: {code}";
        var client = new SmtpClient(_smtpConfiguration.Value.Server, _smtpConfiguration.Value.Port)
        {
            Credentials = new NetworkCredential(_smtpConfiguration.Value.Email, _smtpConfiguration.Value.Password),
            EnableSsl = true
        };
        client.Send(message);
    }

    private string GenerateCode()
        => new Random().Next(1000, 9999).ToString();
}