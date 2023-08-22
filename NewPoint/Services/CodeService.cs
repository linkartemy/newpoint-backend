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

    public override async Task<Response> AddEmailCode(AddEmailCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var code = GenerateCode();
            await _codeRepository.AddEmailCode(request.Email, code);
            await SmtpHandler.SendEmail(request.Email, "NewPoint: Code verification", $"Code: {code}");

            response.Data = Any.Pack(new AddEmailCodeResponse { Sent = true });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }

    public override async Task<Response> VerifyEmailCode(VerifyEmailCodeRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var verified = await _codeRepository.VerifyEmailCode(request.Email, request.Code);

            response.Data = Any.Pack(new VerifyEmailCodeResponse { Verified = verified });
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