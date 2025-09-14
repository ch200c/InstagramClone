using InstagramClone.Api.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InstagramClone.Api.Controllers;

[Route("/")]
[Authorize]
[ApiController]
[Tags("InstagramClone.Api")]
public class IdentityController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILoggedInUserInformationProvider _loggedInUserInformationProvider;

    public IdentityController(SignInManager<IdentityUser> signInManager, ILoggedInUserInformationProvider loggedInUserInformationProvider)
    {
        _signInManager = signInManager;
        _loggedInUserInformationProvider = loggedInUserInformationProvider;
    }

    [HttpPost("logout")]
    public async Task<OkResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpGet("userId")]
    public Task<string> GetId(CancellationToken cancellationToken)
    {
        return _loggedInUserInformationProvider.GetUserIdAsync(cancellationToken);
    }
}
