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

    [Produces("application/json")]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateMedia(
        [FromBody] UploadMediaRequest request, CancellationToken cancellationToken)
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

    [HttpGet("")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK, "image/jpeg")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMedia([FromQuery] GetMediaRequest request, CancellationToken cancellationToken)
    {
        var destinationStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(request.BucketName)
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

public record UploadMediaRequest(string LocalPath, string BucketName);
public record GetMediaRequest(string BucketName, string ObjectName);