using InstagramClone.Api.Application;
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
    private readonly IBucketNameProvider _bucketNameProvider;

    public MediaController(IMinioClient minioClient, IBucketNameProvider bucketNameProvider)
    {
        _minioClient = minioClient;
        _bucketNameProvider = bucketNameProvider;
    }

    [Produces("application/json")]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateMedia(
        [FromBody] UploadMediaRequest request, CancellationToken cancellationToken)
    {
        var bucketName = await _bucketNameProvider.GetBucketNameAsync(cancellationToken);
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
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
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(Path.GetFileName(request.LocalPath))
            .WithFileName(request.LocalPath);

        var response = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        if (response.ResponseStatusCode == HttpStatusCode.OK)
        {
            return Created();
        }

        return BadRequest(response);
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK, "image/jpeg")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMedia([FromQuery] GetMediaRequest request, CancellationToken cancellationToken)
    {
        var bucketName = await _bucketNameProvider.GetBucketNameAsync(cancellationToken);
        var destinationStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(request.ObjectName)
            .WithCallbackStream(async (s, ct) =>
            {
                await s.CopyToAsync(destinationStream, ct);
            });

        try
        {
            await _minioClient.GetObjectAsync(args, cancellationToken);
        }
        catch (Exception ex) when (ex is InvalidObjectNameException || ex is InvalidBucketNameException)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex) when (ex is ObjectNotFoundException || ex is BucketNotFoundException)
        {
            return NotFound();
        }

        destinationStream.Position = 0;
        return File(destinationStream, "image/jpeg");
    }
}

public record UploadMediaRequest(string LocalPath);
public record GetMediaRequest(string ObjectName);