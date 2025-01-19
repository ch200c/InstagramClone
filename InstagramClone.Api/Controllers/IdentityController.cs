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

    public IdentityController(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpPost("logout")]
    public async Task<OkResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }
}
