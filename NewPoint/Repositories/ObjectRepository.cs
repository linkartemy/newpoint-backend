using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using NewPoint.Handlers;

namespace NewPoint.Repositories;

public class ObjectRepository : IObjectRepository
{
    private readonly IMinioClient _minioClient;

    public ObjectRepository(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<bool> ObjectExists(string objectName)
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(S3Handler.Configuration.UserImagesBucket);
            var found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
            if (!found)
            {
                return false;
            }
            var args = new GetObjectArgs().WithBucket(S3Handler.Configuration.UserImagesBucket).WithObject(objectName);
            var obj = await _minioClient.GetObjectAsync(args).ConfigureAwait(false);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<byte[]> GetObjectByName(string name)
    {
        var beArgs = new BucketExistsArgs()
            .WithBucket(S3Handler.Configuration.UserImagesBucket);
        var found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(true);
        if (!found)
        {
            throw new BucketNotFoundException(
                $"No bucket with name {S3Handler.Configuration.UserImagesBucket} was found");
        }

        var data = Array.Empty<byte>();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(S3Handler.Configuration.UserImagesBucket)
            .WithObject(name)
            .WithCallbackStream((stream) =>
            {
                data = StreamHandler.StreamToBytes(stream);
            });
        await _minioClient.GetObjectAsync(getObjectArgs).ConfigureAwait(false);
        return data;
    }

    public async Task<string> InsertObject(byte[] data, string objectName, string contentType = "image/jpeg")
    {
        var beArgs = new BucketExistsArgs()
            .WithBucket(S3Handler.Configuration.UserImagesBucket);
        var found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs()
                .WithBucket(S3Handler.Configuration.UserImagesBucket);
            await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(S3Handler.Configuration.UserImagesBucket)
            .WithObject(objectName)
            .WithFileName(objectName)
            .WithContentType(contentType);
        var response = await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        return response.ObjectName;
    }
}

public interface IObjectRepository
{
    Task<bool> ObjectExists(string objectName);
    Task<byte[]> GetObjectByName(string name);
    Task<string> InsertObject(byte[] data, string objectName, string contentType = "image/jpeg");
}