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

        string accessToken = await _tokenService.GenerateToken(newUser);
        string refreshToken = _tokenService.GenerateRefreshToken();

        await SaveRefreshTokenAsync(newUser.Id, refreshToken);

        return new RegisterUserResponse(accessToken, refreshToken);
    }

    public async Task<LoginUserResponse> LoginAsync(LoginUserRequest userRequest)
    {
        var user = await _userManager.FindByNameAsync(userRequest.Username);
        if (user is null || !await _userManager.CheckPasswordAsync(user, userRequest.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        string accessToken = await _tokenService.GenerateToken(user);
        string refreshToken = _tokenService.GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, refreshToken);

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

    private async Task SaveRefreshTokenAsync(long userId, string refreshToken)
    {
        int expirationDays = _jwtConfigOptions.Value.RefreshTokenExpirationDays;
        var tokenEntity = new RefreshToken
        {
            TokenHash = HashToken(refreshToken),
            UserId = userId,
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