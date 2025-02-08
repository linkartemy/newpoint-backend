using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.VerificationAPI.Repositories;

namespace NewPoint.VerificationAPI.Services
{
    public static class CodeServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
        public const string EitherEmailOrPhoneMustBeFilled = "Either email or phone must be filled";
    }

    public static class CodeServiceErrorCodes
    {
        public const string GenericError = "generic_error";
        public const string EitherEmailOrPhoneMustBeFilled = "either_email_or_phone_must_be_filled";
    }

    public class CodeService : GrpcCode.GrpcCodeBase
    {
        private readonly ILogger<CodeService> _logger;
        private readonly ICodeRepository _codeRepository;

        public CodeService(ICodeRepository codeRepository, ILogger<CodeService> logger)
        {
            _codeRepository = codeRepository;
            _logger = logger;
        }

        public override async Task<AddEmailVerificationCodeResponse> AddEmailVerificationCode(AddEmailVerificationCodeRequest request, ServerCallContext context)
        {
            var email = request.Email.Trim();
            var code = GenerateCode();
            var codeData = new CodeData
            {
                Email = email,
                Code = code,
                CodeType = CodeType.EmailVerification
            };

            try
            {
                await _codeRepository.AddCode(codeData);
                await SmtpHandler.SendEmail(email, "NewPoint: email verification", $"Code: {code}");
                return new AddEmailVerificationCodeResponse { Sent = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddEmailVerificationCode");
                throw new RpcException(new Status(StatusCode.Internal, CodeServiceErrorCodes.GenericError), message: CodeServiceErrorMessages.GenericError);
            }
        }

        public override async Task<VerifyEmailVerificationCodeResponse> VerifyEmailVerificationCode(VerifyEmailVerificationCodeRequest request, ServerCallContext context)
        {
            try
            {
                var codeData = new CodeData
                {
                    Email = request.Email.Trim(),
                    Code = request.Code.Trim(),
                    CodeType = CodeType.EmailVerification
                };

                var verified = await _codeRepository.VerifyCode(codeData);
                return new VerifyEmailVerificationCodeResponse { Verified = verified };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyEmailVerificationCode");
                throw new RpcException(new Status(StatusCode.Internal, CodeServiceErrorCodes.GenericError), message: CodeServiceErrorMessages.GenericError);
            }
        }

        public override async Task<AddPhoneVerificationCodeResponse> AddPhoneVerificationCode(AddPhoneVerificationCodeRequest request, ServerCallContext context)
        {
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
                // Здесь предполагается, что для верификации телефона используется похожий механизм отправки уведомлений.
                await SmtpHandler.SendEmail(phone, "NewPoint: phone verification", $"Code: {code}");
                return new AddPhoneVerificationCodeResponse { Sent = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddPhoneVerificationCode");
                throw new RpcException(new Status(StatusCode.Internal, CodeServiceErrorCodes.GenericError), message: CodeServiceErrorMessages.GenericError);
            }
        }

        public override async Task<VerifyPhoneVerificationCodeResponse> VerifyPhoneVerificationCode(VerifyPhoneVerificationCodeRequest request, ServerCallContext context)
        {
            try
            {
                var codeData = new CodeData
                {
                    Phone = request.Phone.Trim(),
                    Code = request.Code.Trim(),
                    CodeType = CodeType.PhoneVerification
                };

                var verified = await _codeRepository.VerifyCode(codeData);
                return new VerifyPhoneVerificationCodeResponse { Verified = verified };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyPhoneVerificationCode");
                throw new RpcException(new Status(StatusCode.Internal, CodeServiceErrorCodes.GenericError), message: CodeServiceErrorMessages.GenericError);
            }
        }

        public override async Task<AddPasswordChangeVerificationCodeResponse> AddPasswordChangeVerificationCode(AddPasswordChangeVerificationCodeRequest request, ServerCallContext context)
        {

            if (!request.HasEmail && !request.HasPhone)
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, CodeServiceErrorCodes.EitherEmailOrPhoneMustBeFilled),
                    message: CodeServiceErrorMessages.EitherEmailOrPhoneMustBeFilled
                );
            }
            var codeData = new CodeData
            {
                CodeType = CodeType.PhoneVerification, // Возможно, для password change стоит завести отдельный тип
                Phone = request.HasPhone ? request.Phone.Trim() : null,
                Email = request.HasEmail ? request.Email.Trim() : null
            };
            try
            {
                var code = GenerateCode();
                codeData.Code = code;

                await _codeRepository.AddCode(codeData);

                var title = "NewPoint: password change verification";
                var text = $"Code: {code}.\nIf you haven't changed your password, please contact us.";
                await SmtpHandler.SendEmail(codeData.Email ?? codeData.Phone!, title, text);

                return new AddPasswordChangeVerificationCodeResponse { Sent = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddPasswordChangeVerificationCode");
                throw new RpcException(new Status(StatusCode.Internal, CodeServiceErrorCodes.GenericError), message: CodeServiceErrorMessages.GenericError);
            }
        }

        public override async Task<VerifyPasswordChangeVerificationCodeResponse> VerifyPasswordChangeVerificationCode(VerifyPasswordChangeVerificationCodeRequest request, ServerCallContext context)
        {
            if (!request.HasEmail && !request.HasPhone)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, CodeServiceErrorCodes.EitherEmailOrPhoneMustBeFilled), message: CodeServiceErrorMessages.EitherEmailOrPhoneMustBeFilled);
            }
            try
            {
                var codeData = new CodeData
                {
                    Code = request.Code.Trim(),
                    CodeType = CodeType.PhoneVerification, // Возможно, здесь также нужен отдельный тип
                    Phone = request.HasPhone ? request.Phone.Trim() : null,
                    Email = request.HasEmail ? request.Email.Trim() : null
                };

                var verified = await _codeRepository.VerifyCode(codeData);
                return new VerifyPasswordChangeVerificationCodeResponse { Verified = verified };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyPasswordChangeVerificationCode");
                throw new RpcException(new Status(StatusCode.Internal, CodeServiceErrorCodes.GenericError), message: CodeServiceErrorMessages.GenericError);
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
}
