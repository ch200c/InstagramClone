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
    private readonly ILoggedInUserInformationProvider _loggedInUserInformationProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MediaController(
        IMinioClient minioClient, 
        IBucketNameProvider bucketNameProvider, 
        ApplicationDbContext dbContext, 
        ILoggedInUserInformationProvider loggedInUserInformationProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _minioClient = minioClient;
        _bucketNameProvider = bucketNameProvider;
        _dbContext = dbContext;
        _loggedInUserInformationProvider = loggedInUserInformationProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    [Produces("application/json")]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateMedia(
        [FromBody] UploadMediaRequest request, CancellationToken cancellationToken)
    {
        var userId = await _loggedInUserInformationProvider.GetUserIdAsync(cancellationToken);
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
            var userMedia = new UserMedia()
            {
                BucketName = bucketName,
                ObjectName = response.ObjectName,
                Title = request.Title,
                UserId = userId,
                CreatedAt = _dateTimeProvider.GetDateTime()
            };
            _dbContext.UserMedia.Add(userMedia);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetMedia), new { id = userMedia.Id }, userMedia.Id);
        }

        return BadRequest(response);
    }

    [HttpGet("{id}/preview")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK, "image/jpeg")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PreviewMedia([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userMedia = await _dbContext.UserMedia.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

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

    [Produces("application/json")]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMedia([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userMedia = await _dbContext.UserMedia.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (userMedia == null)
        {
            return NotFound();
        }

        var response = new GetMediaResponse(id, userMedia.Title);
        return Ok(response);
    }

    [Produces("application/json")]
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SearchMedia([FromQuery] SearchMediaRequest request, CancellationToken cancellationToken)
    {
        var matches = await _dbContext.UserMedia
            .Where(u => u.Title != null && u.Title.Contains(request.Title))
            .Take(10)
            .OrderBy(u => u.Title)
            .Select(u => new { u.Id, u.Title })
            .ToListAsync(cancellationToken);

        var response = new SearchMediaResult(matches.Select(m => new GetMediaResponse(m.Id, m.Title)));
        return Ok(response);
    }
}

public record UploadMediaRequest(string LocalPath, string Title);
public record GetMediaResponse(Guid Id, string? Title);
public record SearchMediaRequest(string Title);
public record SearchMediaResult(IEnumerable<GetMediaResponse> Matches);