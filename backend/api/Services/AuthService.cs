using api.Configuration;
using api.Data;
using api.Dtos;
using api.Entities;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace api.Services;

internal class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDateTimeOffsetProvider _dateTimeOffsetProvider;
    private readonly IOptions<JwtConfig> _jwtConfigOptions;
    
    public AuthService(
        ITokenService tokenService, 
        UserManager<User> userManager, 
        AppDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        IDateTimeOffsetProvider dateTimeOffsetProvider,
        IOptions<JwtConfig> jwtConfigOptions)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
        _dateTimeOffsetProvider = dateTimeOffsetProvider;
        _jwtConfigOptions = jwtConfigOptions;
    }

    public async Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest userRequest)
    {
        var existingUser = await _userManager.FindByNameAsync(userRequest.Username);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("User already exists.");
        }

        var utcNow = _dateTimeOffsetProvider.UtcNow;
        var newUser = new User
        {
            UserName = userRequest.Username,
            Email = userRequest.Email,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var result = await _userManager.CreateAsync(newUser, userRequest.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        var sessionId = Guid.NewGuid();
        var loginDate = _dateTimeProvider.UtcNow;
        string accessToken = await _tokenService.GenerateToken(newUser, sessionId, loginDate);
        string refreshToken = _tokenService.GenerateRefreshToken();

        await SaveRefreshTokenAsync(newUser.Id, refreshToken, sessionId, loginDate);

        return new RegisterUserResponse(accessToken, refreshToken);
    }

    public async Task<LoginUserResponse> LoginAsync(LoginUserRequest userRequest)
    {
        var user = await _userManager.FindByNameAsync(userRequest.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, userRequest.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var sessionId = Guid.NewGuid();
        var loginDate = _dateTimeProvider.UtcNow;
        string accessToken = await _tokenService.GenerateToken(user, sessionId, loginDate);
        string refreshToken = _tokenService.GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, refreshToken, sessionId, loginDate);

        return new LoginUserResponse(accessToken, refreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        string hashedToken = HashToken(refreshToken);
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);
        if (token is not null)
        {
            _dbContext.RefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<RefreshTokenResponse> RefreshAsync(RefreshTokenRequest request)
    {
        // #TODO: improve to handle multiple refresh tokens per user
        // For now, editing existing refresh token instead of creating a new one, for simplicity.
        string hashedToken = HashToken(request.RefreshToken);

        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken);

        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }
        if (refreshToken.ExpiresOnUtc < _dateTimeProvider.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token has expired.");
        }

        string newAccessToken = await _tokenService.GenerateToken(refreshToken.User, refreshToken.Id, refreshToken.CreatedAtUtc);
        string newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        
        int tokenExpirationDays = _jwtConfigOptions.Value.RefreshTokenExpirationDays;
        refreshToken.ExpiresOnUtc = _dateTimeProvider.UtcNow.AddDays(tokenExpirationDays);
        refreshToken.TokenHash = HashToken(newRefreshTokenValue);
        
        await _dbContext.SaveChangesAsync();

        return new RefreshTokenResponse(newAccessToken, newRefreshTokenValue);
    }

    private async Task SaveRefreshTokenAsync(long userId, string refreshToken, Guid sessionId, DateTime createdAtUtc)
    {
        int expirationDays = _jwtConfigOptions.Value.RefreshTokenExpirationDays;
        var tokenEntity = new RefreshToken
        {
            Id = sessionId,
            TokenHash = HashToken(refreshToken),
            UserId = userId,
            CreatedAtUtc = createdAtUtc,
            ExpiresOnUtc = _dateTimeProvider.UtcNow.AddDays(expirationDays)
        };

        _dbContext.RefreshTokens.Add(tokenEntity);
        await _dbContext.SaveChangesAsync();
    }

    private static string HashToken(string token)
    {
        // #TODO: use a better hashing algorithm
        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashedToken = SHA256.HashData(tokenBytes);
        return Convert.ToBase64String(hashedToken);
    }
}