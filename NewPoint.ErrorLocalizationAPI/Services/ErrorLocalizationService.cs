using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.StaticFiles;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.ErrorLocalizationAPI.Clients;
using NewPoint.ErrorLocalizationAPI.Extensions;

namespace NewPoint.ErrorLocalizationAPI.Services;

public class ErrorLocalizationService : GrpcErrorLocalization.GrpcErrorLocalizationBase
{
    private readonly ILogger<ErrorLocalizationService> _logger;
    private readonly IRedisClient _redisClient;

    public ErrorLocalizationService(IRedisClient redisClient, ILogger<ErrorLocalizationService> logger)
    {
        _redisClient = redisClient;
        _logger = logger;
    }

    public override async Task<ErrorMessageResponse> GetErrorMessage(GetErrorMessageRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.ErrorCode) || string.IsNullOrWhiteSpace(request.Language))
        {
            _logger.LogWarning("Invalid request: ErrorCode or Language is missing");
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ErrorCode and Language must be provided."));
        }

        try
        {
            if (await _redisClient.ErrorLocalizationExists(request.ErrorCode, request.Language) is false)
            {
                _logger.LogInformation($"Localization not found for error: {request.ErrorCode}, language: {request.Language}");
                throw new RpcException(new Status(StatusCode.NotFound, "Localization wasn't found."));
            }

            var localizedMessage = await _redisClient.GetErrorLocalization(request.ErrorCode, request.Language);
            if (string.IsNullOrEmpty(localizedMessage))
            {
                _logger.LogInformation($"Localization not found for error: {request.ErrorCode}, language: {request.Language}");
                throw new RpcException(new Status(StatusCode.NotFound, "Localization not found"));
            }

            return new ErrorMessageResponse
            {
                ErrorCode = request.ErrorCode,
                Message = localizedMessage,
                Language = request.Language
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localization from Redis");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}