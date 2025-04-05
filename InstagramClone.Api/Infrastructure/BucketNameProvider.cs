using InstagramClone.Api.Application;
using InstagramClone.Api.Domain.Entities;
using InstagramClone.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Api.Infrastructure;

public class BucketNameProvider : IBucketNameProvider
{
    private readonly ILoggedInUserInformationProvider _loggedInUserInformationProvider;
    private readonly ApplicationDbContext _dbContext;

    public BucketNameProvider(
        ILoggedInUserInformationProvider loggedInUserInformationProvider, ApplicationDbContext dbContext)
    {
        _loggedInUserInformationProvider = loggedInUserInformationProvider;
        _dbContext = dbContext;
    }

    public async Task<string> GetBucketNameAsync(CancellationToken cancellationToken)
    {
        var userId = await _loggedInUserInformationProvider.GetUserIdAsync(cancellationToken);

        var existingUserBucketName = await _dbContext.UserBucketNames.SingleOrDefaultAsync(
            u => u.UserId == userId, cancellationToken);

        if (existingUserBucketName == null)
        {
            var userBucketName = new UserBucketName()
            {
                BucketName = Guid.NewGuid().ToString(),
                UserId = userId
            };

            _dbContext.UserBucketNames.Add(userBucketName);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return userBucketName.BucketName;
        }

        return existingUserBucketName.BucketName;
    }
}
