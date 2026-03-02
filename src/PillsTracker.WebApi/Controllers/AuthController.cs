using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillsTracker.Contracts.Auth;
using PillsTracker.Infrastructure.Identity;
using PillsTracker.Infrastructure.Persistence;
using PillsTracker.WebApi.Auth;

namespace PillsTracker.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    PillsTrackerDbContext dbContext,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized();

        if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
        {
            user.LastKnownTimeZoneId = request.TimeZoneId;
            await userManager.UpdateAsync(user);
        }

        return await IssueTokensAsync(user);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
    {
        var hash = tokenService.ComputeHash(request.RefreshToken);
        var existing = await dbContext.RefreshTokens
            .Where(x => x.TokenHash == hash && !x.IsRevoked)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (existing is null)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null)
            return Unauthorized();

        existing.IsRevoked = true;
        existing.RevokedAtUtc = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.TimeZoneId))
        {
            user.LastKnownTimeZoneId = request.TimeZoneId;
            await userManager.UpdateAsync(user);
        }

        var response = await IssueTokensAsync(user);
        await dbContext.SaveChangesAsync();
        return response;
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null) return NoContent();

        var tokens = await dbContext.RefreshTokens.Where(x => x.UserId == user.Id && !x.IsRevoked).ToListAsync();
        foreach (var t in tokens)
        {
            t.IsRevoked = true;
            t.RevokedAtUtc = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user)
    {
        var access = tokenService.CreateAccessToken(user);
        var refresh = tokenService.CreateRefreshToken();

        await dbContext.RefreshTokens.AddAsync(new RefreshTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.ComputeHash(refresh),
            IsRevoked = false,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        return new AuthResponse(access, refresh);
    }
}
