using MaintenancePortal.Api.Contracts.Auth;
using MaintenancePortal.Core.Abstractions;
using MaintenancePortal.Core.Constants;
using MaintenancePortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MaintenancePortal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    private static readonly string[] AllowedRoles = [Roles.Landlord, Roles.Tenant];

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (!AllowedRoles.Contains(request.Role))
        {
            return BadRequest($"Role must be one of: {string.Join(", ", AllowedRoles)}");
        }

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return Conflict("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        // UserManager handles password hashing via Identity - we never touch
        // password hashes directly.
        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            // Deliberately identical response for "no such user" and "wrong
            // password" - don't leak which one it was.
            return Unauthorized("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, roles);

        return Ok(new AuthResponse
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles
        });
    }
}
