using System.Text.RegularExpressions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NewPoint.Common.Extensions;
using NewPoint.Common.Handlers;
using NewPoint.Common.Models;
using NewPoint.ObjectAPI.Repositories;

namespace NewPoint.ObjectAPI.Services
{
    public static class ImageServiceErrorMessages
    {
        public const string GenericError = "Something went wrong. Please try again later. We are sorry";
        public const string ImageNotFound = "Image not found";
    }

    public static class ImageServiceErrorCodes
    {
        public const string GenericError = "generic_error";
        public const string ImageNotFound = "image_not_found";
    }

    public class ImageService : GrpcImage.GrpcImageBase
    {
        private readonly ILogger<ImageService> _logger;
        private readonly IImageRepository _imageRepository;
        private readonly IObjectRepository _objectRepository;

        public ImageService(
            IImageRepository imageRepository,
            IObjectRepository objectRepository,
            ILogger<ImageService> logger)
        {
            _imageRepository = imageRepository;
            _objectRepository = objectRepository;
            _logger = logger;
        }

        public override async Task<GetImageByIdResponse> GetImageById(GetImageByIdRequest request, ServerCallContext context)
        {
            if (!await _imageRepository.ImageExists(request.Id))
            {
                throw new RpcException(
                    new Status(StatusCode.NotFound, ImageServiceErrorCodes.ImageNotFound),
                    ImageServiceErrorMessages.ImageNotFound);
            }

            try
            {
                var name = await _imageRepository.GetImageNameById(request.Id);
                var image = await _objectRepository.GetObjectByName("user-images", name);

                return new GetImageByIdResponse
                {
                    Image = new ImageModel
                    {
                        Id = request.Id,
                        Data = ByteString.CopyFrom(image)
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting image by id");
                throw new RpcException(
                    new Status(StatusCode.Internal, ImageServiceErrorCodes.GenericError),
                    ImageServiceErrorMessages.GenericError);
            }
        }
    }
}
