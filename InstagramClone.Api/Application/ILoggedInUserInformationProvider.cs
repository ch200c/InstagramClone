namespace InstagramClone.Api.Application;

public interface ILoggedInUserInformationProvider
{
    Task<string> GetUserIdAsync(CancellationToken cancellationToken);
}
