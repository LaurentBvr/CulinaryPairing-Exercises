using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CulinaryPairing.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryPairing.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login).RequireRateLimiting("login");
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<CulinaryUser> userManager)
    {
        var user = new CulinaryUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        await userManager.AddToRoleAsync(user, "FamilyMember");

        return Results.Ok(new { Message = "Inscription reussie", UserId = user.Id });
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<CulinaryUser> userManager,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var logger = loggerFactory.CreateLogger("Auth");
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            // F2 : Log connexion echouee (JAMAIS le mot de passe)
            logger.LogWarning("Login echoue pour {Email} depuis {IP}", request.Email, ip);
            return Results.Unauthorized();
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            // F3 : Log verrouillage
            logger.LogWarning("Compte verrouille pour {Email} depuis {IP}", request.Email, ip);
            return Results.Problem("Compte verrouille. Reessayez plus tard.", statusCode: 423);
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles, configuration);

        // F1 : Log connexion reussie
        logger.LogInformation("Login reussi pour {Email} avec roles {Roles} depuis {IP}",
            user.Email, string.Join(",", roles), ip);

        return Results.Ok(new LoginResponse(token, user.Email!, user.FullName, roles.ToList()));
    }

    private static async Task<IResult> GetCurrentUser(
        ClaimsPrincipal principal,
        UserManager<CulinaryUser> userManager)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return Results.NotFound();

        var roles = await userManager.GetRolesAsync(user);

        return Results.Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            Roles = roles
        });
    }

    private static string GenerateJwtToken(
        CulinaryUser user, IList<string> roles, IConfiguration config)
    {
        var jwtConfig = config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwtConfig["ExpirationInMinutes"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string FullName, List<string> Roles);