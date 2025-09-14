using InstagramClone.Api.Application;
using InstagramClone.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Api.Controllers;

[Route("api/feed")]
[ApiController]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILoggedInUserInformationProvider _loggedInUserInformationProvider;

    public FeedController(ApplicationDbContext dbContext, ILoggedInUserInformationProvider loggedInUserInformationProvider)
    {
        _dbContext = dbContext;
        _loggedInUserInformationProvider = loggedInUserInformationProvider;
    }

    [Produces("application/json")]
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetFeed(CancellationToken cancellationToken)
    {
        var userId = await _loggedInUserInformationProvider.GetUserIdAsync(cancellationToken);

        var followedUserIds = await _dbContext.UserFollowing
            .Where(u => u.FollowerUserId == userId)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken);

        var media = await _dbContext.UserMedia
            .Where(m => followedUserIds.Contains(m.UserId))
            .Take(10)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new GetMediaResponse(m.Id, m.Title, m.CreatedAt))
            .ToListAsync(cancellationToken);

        var response = new GetMediaListResponse(media);
        return Ok(response);
    }
}
