namespace InstagramClone.Api.Application;

public interface IBucketNameProvider
{
    Task<string> GetBucketNameAsync(CancellationToken cancellationToken);
}
