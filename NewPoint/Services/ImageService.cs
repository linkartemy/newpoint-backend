using System.Text.RegularExpressions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Extensions;
using NewPoint.Handlers;
using NewPoint.Models;
using NewPoint.Repositories;

namespace NewPoint.Services;

public class ImageService : GrpcImage.GrpcImageBase
{
    private readonly ILogger<ImageService> _logger;
    private readonly IImageRepository _imageRepository;

    public ImageService(IImageRepository imageRepository, ILogger<ImageService> logger)
    {
        _imageRepository = imageRepository;
        _logger = logger;
    }

    public override async Task<Response> GetImageById(GetImageByIdRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            if (await _imageRepository.ImageExists(request.Id) is false)
            {
                response.Error = "Image doesn't exist";
                response.Status = 400;
                return response;
            }

            var image = await _imageRepository.GetImageById(request.Id);

            response.Data = Any.Pack(new GetImageByIdResponse
            {
                Image = new ImageModel
                {
                    Image = ByteString.CopyFrom(image)
                }
            });
            return response;
        }
        catch (Exception)
        {
            response.Status = 500;
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return response;
        }
    }
}