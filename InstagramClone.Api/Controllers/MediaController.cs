using InstagramClone.Api.Application;
using InstagramClone.Api.Domain.Entities;
using InstagramClone.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _dbContext;

    public MediaController(IMinioClient minioClient, IBucketNameProvider bucketNameProvider, ApplicationDbContext dbContext)
    {
        _minioClient = minioClient;
        _bucketNameProvider = bucketNameProvider;
        _dbContext = dbContext;
    }

    [Produces("application/json")]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateMedia(
        [FromBody] UploadMediaRequest request, CancellationToken cancellationToken)
    {
        var bucketName = await _bucketNameProvider.GetBucketNameAsync(cancellationToken);
        var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);

        try
        {
            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
        }
        catch (InvalidBucketNameException ex)
        {
            return BadRequest(ex.ServerMessage);
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(Path.GetFileName(request.LocalPath))
            .WithFileName(request.LocalPath);

        var response = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        if (response.ResponseStatusCode == HttpStatusCode.OK)
        {
            var userMedia = new UserMedia() { BucketName = bucketName, ObjectName = response.ObjectName };
            _dbContext.UserMedia.Add(userMedia);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetMedia), new { Id = userMedia.Id }, userMedia.Id);
        }

        return BadRequest(response);
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK, "image/jpeg")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMedia([FromQuery] GetMediaRequest request, CancellationToken cancellationToken)
    {
        var userMedia = await _dbContext.UserMedia.SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (userMedia == null)
        {
            return NotFound();
        }

        var destinationStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(userMedia.BucketName)
            .WithObject(userMedia.ObjectName)
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
public record GetMediaRequest(Guid Id);