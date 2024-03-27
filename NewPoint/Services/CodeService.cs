using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class CodeService : GrpcCode.GrpcCodeBase
{
    private readonly ILogger<CodeService> _logger;
    private readonly ICodeRepository _codeRepository;

    public CodeService(ICodeRepository codeRepository, ILogger<CodeService> logger)
    {
        _codeRepository = codeRepository;
        _logger = logger;
    }

    public override async Task<Response> AddEmailVerificationCode(AddEmailVerificationCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var email = request.Email.Trim();
            var code = GenerateCode();
            var codeData = new CodeData
            {
                Email = email,
                Code = code,
                CodeType = CodeType.EmailVerification
            };
            await _codeRepository.AddCode(codeData);
            await SmtpHandler.SendEmail(email, "NewPoint: email verification", $"Code: {code}");

            response.Data = Any.Pack(new AddEmailVerificationCodeResponse { Sent = true });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> VerifyEmailVerificationCode(VerifyEmailVerificationCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var codeData = new CodeData
            {
                Email = request.Email.Trim(),
                Code = request.Code.Trim(),
                CodeType = CodeType.EmailVerification
            };
            var verified = await _codeRepository.VerifyCode(codeData);

            response.Data = Any.Pack(new VerifyEmailVerificationCodeResponse { Verified = verified });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> AddPhoneVerificationCode(AddPhoneVerificationCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var phone = request.Phone.Trim();
            var code = GenerateCode();
            var codeData = new CodeData
            {
                Phone = phone,
                Code = code,
                CodeType = CodeType.PhoneVerification
            };
            await _codeRepository.AddCode(codeData);
            await SmtpHandler.SendEmail(phone, "NewPoint: email verification", $"Code: {code}");

            response.Data = Any.Pack(new AddPhoneVerificationCodeResponse { Sent = true });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> VerifyPhoneVerificationCode(VerifyPhoneVerificationCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var codeData = new CodeData
            {
                Phone = request.Phone.Trim(),
                Code = request.Code.Trim(),
                CodeType = CodeType.PhoneVerification
            };
            var verified = await _codeRepository.VerifyCode(codeData);

            response.Data = Any.Pack(new VerifyPhoneVerificationCodeResponse { Verified = verified });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> AddPasswordChangeVerificationCode(AddPasswordChangeVerificationCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var codeData = new CodeData
            {
                CodeType = CodeType.PhoneVerification
            };

            if (request.Email.KindCase is NullableString.KindOneofCase.Null && request.Phone.KindCase is NullableString.KindOneofCase.Null)
            {
                response.Status = 400;
                response.Error = "Either email or phone must be filled";
                return response;
            }

            codeData.Phone = request.Phone.KindCase is NullableString.KindOneofCase.Null ? null : request.Phone.Data.Trim();
            codeData.Email = request.Email.KindCase is NullableString.KindOneofCase.Null ? null : request.Email.Data.Trim();

            var code = GenerateCode();
            codeData.Code = code;
            await _codeRepository.AddCode(codeData);

            var title = "NewPoint: password change verification";
            var text = $"Code: {code}.\nIf you haven't changed password, please contact us.";
            await SmtpHandler.SendEmail(codeData.Email ?? codeData.Phone!, title, text);

            response.Data = Any.Pack(new AddPasswordChangeVerificationCodeResponse { Sent = true });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> VerifyPasswordChangeVerificationCode(VerifyPasswordChangeVerificationCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var codeData = new CodeData
            {
                Code = request.Code.Trim(),
                CodeType = CodeType.PhoneVerification
            };

            if (request.Email.KindCase is NullableString.KindOneofCase.Null && request.Phone.KindCase is NullableString.KindOneofCase.Null)
            {
                response.Status = 400;
                response.Error = "Either email or phone must be filled";
                return response;
            }

            codeData.Phone = request.Phone.KindCase is NullableString.KindOneofCase.Null ? null : request.Phone.Data.Trim();
            codeData.Email = request.Email.KindCase is NullableString.KindOneofCase.Null ? null : request.Email.Data.Trim();

            var verified = await _codeRepository.VerifyCode(codeData);

            response.Data = Any.Pack(new VerifyPasswordChangeVerificationCodeResponse { Verified = verified });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    private string GenerateCode()
    {
        var min = 1000;
        var max = 9999;
        var random = new Random();
        return random.Next(min, max).ToString();
    }
}