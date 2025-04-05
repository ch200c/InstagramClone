using InstagramClone.Api.Application;
using InstagramClone.Api.Domain.Entities;
using InstagramClone.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Api.Controllers;

[Route("api/userFollowing")]
[ApiController]
[Authorize]
public class UserFollowingController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILoggedInUserInformationProvider _loggedInUserInformationProvider;

    public UserFollowingController(
        ApplicationDbContext dbContext, ILoggedInUserInformationProvider loggedInUserInformationProvider)
    {
        _dbContext = dbContext;
        _loggedInUserInformationProvider = loggedInUserInformationProvider;
    }

    [Produces("application/json")]
    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateUserFollowing(
        [FromBody] CreateUserFollowingRequest request, CancellationToken cancellationToken)
    {
        var isValidUser = await _dbContext.Users
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!isValidUser)
        {
            return BadRequest("User not found");
        }

        var loggedInUserId = await _loggedInUserInformationProvider.GetUserIdAsync(cancellationToken);

        if (loggedInUserId == request.UserId)
        {
            return BadRequest("You cannot follow yourself");
        }

        var isExistingFollowing = await _dbContext.UserFollowing
            .AnyAsync(u => u.UserId == request.UserId
                && u.FollowerUserId == loggedInUserId, cancellationToken);

        if (isExistingFollowing)
        {
            return BadRequest("Following already exists");
        }

        var userFollowing = new UserFollowing()
        {
            FollowerUserId = loggedInUserId,
            UserId = request.UserId
        };

        _dbContext.UserFollowing.Add(userFollowing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetUserFollowing), new { id = userFollowing.Id }, Map(userFollowing));
    }

    [Produces("application/json")]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserFollowing([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userFollowing = await _dbContext.UserFollowing.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (userFollowing == null)
        {
            return NotFound();
        }

        var response = Map(userFollowing);
        return Ok(response);
    }

    private static GetUserFollowingResponse Map(UserFollowing userFollowing)
    {
        return new GetUserFollowingResponse(userFollowing.Id, userFollowing.UserId, userFollowing.FollowerUserId);
    }
}

public record CreateUserFollowingRequest(string UserId);
public record GetUserFollowingResponse(Guid Id, string UserId, string FollowerUserId);