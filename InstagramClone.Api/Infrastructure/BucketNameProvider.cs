using InstagramClone.Api.Application;
using InstagramClone.Api.Domain.Entities;
using InstagramClone.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Api.Infrastructure;

public class BucketNameProvider : IBucketNameProvider
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _dbContext;

    public BucketNameProvider(
        UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<string> GetBucketNameAsync(CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new BucketNameProviderException("Could not get HttpContext");

        var identityUser = await _userManager.GetUserAsync(user)
            ?? throw new BucketNameProviderException("Could not get Identity user");

        var existingUserBucketName = await _dbContext.UserBucketNames.SingleOrDefaultAsync(
            u => u.IdentityUserId == identityUser.Id, cancellationToken);

        if (existingUserBucketName == null)
        {
            var userBucketName = new UserBucketName()
            {
                BucketName = Guid.NewGuid().ToString(),
                IdentityUserId = identityUser.Id
            };

            _dbContext.UserBucketNames.Add(userBucketName);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return userBucketName.BucketName;
        }

        return existingUserBucketName.BucketName;
    }
}
