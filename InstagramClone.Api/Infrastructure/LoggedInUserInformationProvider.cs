using InstagramClone.Api.Application;
using Microsoft.AspNetCore.Identity;

namespace InstagramClone.Api.Infrastructure;

public class LoggedInUserInformationProvider : ILoggedInUserInformationProvider
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggedInUserInformationProvider(
        UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> GetUserIdAsync(CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new BucketNameProviderException("Could not get HttpContext");

        var identityUser = await _userManager.GetUserAsync(user)
            ?? throw new BucketNameProviderException("Could not get Identity user");

        return identityUser.Id;
    }
}
