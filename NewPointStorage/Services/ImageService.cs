using System.Text.RegularExpressions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPointStorage.Extensions;
using NewPointStorage.Handlers;
using NewPointStorage.Repositories;

namespace NewPointStorage.Services;

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

            var hash = await _imageRepository.GetImageHashById(request.Id);

            response.Data = Any.Pack(new GetImageByIdResponse
            {
                Image = new ImageModel
                {
                    Id = request.Id,
                    Hash = hash
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

    public override async Task<Response> AddImage(AddImageRequest request, ServerCallContext context)
    {
        var response = new Response
        {
            Status = 200
        };
        try
        {
            var id = await _imageRepository.InsertImage("");

            response.Data = Any.Pack(new AddImageResponse
            {
                Id = id
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