using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.Configuration;
using api.Entities;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

internal class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly IOptions<JwtConfig> _jwtConfigOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public TokenService(
        UserManager<User> userManager, 
        IOptions<JwtConfig> jwtConfigOptions,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = userManager;
        _jwtConfigOptions = jwtConfigOptions;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public async Task<string> GenerateToken(User user, Guid sessionId, DateTimeOffset loginDateUtc)
    {
        var jwtConfig = _jwtConfigOptions.Value;
        if (jwtConfig is null) throw new InvalidOperationException($"Failed to load {JwtConfig.SectionName} from configuration.");
    
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("session_id", sessionId.ToString()),
            new Claim("login_date", loginDateUtc.ToString("o")) // ISO 8601 format
        };
        var userRoles = await _userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var securityDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Audience = jwtConfig.Audience,
            Issuer = jwtConfig.Issuer,
            IssuedAt = _dateTimeProvider.UtcNow,
            NotBefore = _dateTimeProvider.UtcNow,
            Expires = _dateTimeProvider.UtcNow.AddMinutes(jwtConfig.AccessTokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        };
        
        var jwtHandler = new JsonWebTokenHandler();
        string token = jwtHandler.CreateToken(securityDescriptor);

        return token;
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}