using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Net;

namespace InstagramClone.Api.Controllers;

[Route("api/media")]
[ApiController]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMinioClient _minioClient;

    public MediaController(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateMedia([FromBody] UploadMediaRequest request, CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(request.BucketName);
        bool isExistingBucket;

        try
        {
            isExistingBucket = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
        }
        catch (InvalidBucketNameException ex)
        {
            return BadRequest(ex.ServerMessage);
        }

        if (!isExistingBucket)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(request.BucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(request.BucketName)
            .WithObject(Path.GetFileName(request.LocalPath))
            .WithFileName(request.LocalPath);

        var response = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        if (response.ResponseStatusCode == HttpStatusCode.OK)
        {
            return Created();
        }

        return BadRequest(response);
    }
}


public record UploadMediaRequest(string LocalPath, string BucketName);