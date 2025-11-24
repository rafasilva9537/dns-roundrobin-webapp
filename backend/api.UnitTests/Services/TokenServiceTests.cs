using System.Text;
using api.Configuration;
using api.Constants;
using api.Entities;
using api.Interfaces.Services;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace UnitTests.Services;

public class TokenServiceTests
{
    private readonly UserManager<User> _userManagerMock;
    
    public TokenServiceTests()
    {
        var userStoreMock = Substitute.For<IUserStore<User>>();
        _userManagerMock = Substitute.For<UserManager<User>>(userStoreMock, null, null, null, null, null, null, null, null);
    }
    
    [Theory]
    [InlineData("TestUser", "test_user@gmail.com", "test-domain.com", "test-domain.com")]
    [InlineData("TestUser", "test_user@gmail.com", "test-domain-1.com", "test-domain-2.com")]
    [InlineData("", "", "test-domain.com", "test-domain.com")]
    public async Task GenerateToken_WithValidUser_ReturnsExpectedHeaderAndPayload(string expectedUserName, string expectedEmail, string expectedIssuer, string expectedAudience)
    {
        // Arrange
        var user = new User() { Id = 1, UserName = expectedUserName, Email = expectedEmail };
        
        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        var fakeCreationDate = new DateTime(2024, 11, 24, 06, 24, 45, DateTimeKind.Utc);
        dateTimeProviderMock.UtcNow.Returns(fakeCreationDate);
        
        _userManagerMock.GetRolesAsync(Arg.Any<User>()).Returns([RoleConstants.Admin]);
        
        const string expectedSecret = "a-string-secret-at-least-256-bits-long-used-for-testing";
        const int expectedExpirationMinutes = 15;
        const int expectedExpirationDays = 7;

        var jwtOptionsMock = Substitute.For<IOptions<JwtConfig>>();
        var fakeJwtConfig = new JwtConfig()
        {
            Audience = expectedAudience,
            Issuer = expectedIssuer,
            Secret = expectedSecret,
            AccessTokenExpirationMinutes = expectedExpirationMinutes,
            RefreshTokenExpirationDays = expectedExpirationDays
        };
        jwtOptionsMock.Value.Returns(fakeJwtConfig);
    
        ITokenService tokenService = new TokenService(_userManagerMock, jwtOptionsMock, dateTimeProviderMock);
        
        var tokenHandler = new JsonWebTokenHandler();
    
        
        // Act
        string tokenString = await tokenService.GenerateToken(user, Guid.Empty, DateTimeOffset.MinValue);
        
        
        // Assert
        var actualJwt = tokenHandler.ReadJsonWebToken(tokenString);
        var jwtClaimName = actualJwt.GetClaim(JwtRegisteredClaimNames.Name);
        var jwtClaimEmail = actualJwt.GetClaim(JwtRegisteredClaimNames.Email);
        var jwtClaimAdminRole = actualJwt.Claims.FirstOrDefault(c => c.Value == RoleConstants.Admin);
        
        Assert.Equal(SecurityAlgorithms.HmacSha256, actualJwt.Alg);
        Assert.Equal("JWT", actualJwt.Typ);
        
        Assert.Equal(RoleConstants.Admin, jwtClaimAdminRole?.Value);
        Assert.Equal(expectedUserName, jwtClaimName.Value);
        Assert.Equal(expectedEmail, jwtClaimEmail.Value);
        Assert.Equal(expectedIssuer, actualJwt.Issuer);
        Assert.Single(actualJwt.Audiences);
        Assert.Equal(expectedAudience, actualJwt.Audiences.First());
        Assert.Equal(fakeCreationDate, actualJwt.IssuedAt);
        Assert.Equal(fakeCreationDate, actualJwt.ValidFrom);
        Assert.Equal(fakeCreationDate.AddMinutes(expectedExpirationMinutes), actualJwt.ValidTo);
    }

    [Theory]
    [InlineData("TestUser", "test_user@gmail.com", "test-domain.com", "test-domain.com")]
    [InlineData("", "", "test-domain.com", "test-domain.com")]
    public async Task GenerateToken_WithValidUser_ReturnsValidJwtFormat(string expectedUserName, string expectedEmail, string expectedIssuer, string expectedAudience)
    {
        // Arrange
        var user = new User() { Id = 1, UserName = expectedUserName, Email = expectedEmail };
        
        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        var fakeCreationDate = new DateTime(2024, 11, 24, 06, 24, 45, DateTimeKind.Utc);
        dateTimeProviderMock.UtcNow.Returns(fakeCreationDate);
        
        const string expectedSecret = "a-string-secret-at-least-256-bits-long-used-for-testing";
        const int expectedExpirationMinutes = 15;
        const int expectedExpirationDays = 7;
        
        var jwtOptionsMock = Substitute.For<IOptions<JwtConfig>>();
        var fakeJwtConfig = new JwtConfig()
        {
            Audience = expectedAudience,
            Issuer = expectedIssuer,
            Secret = expectedSecret,
            AccessTokenExpirationMinutes = expectedExpirationMinutes,
            RefreshTokenExpirationDays = expectedExpirationDays
        };
        jwtOptionsMock.Value.Returns(fakeJwtConfig);
    
        ITokenService tokenService = new TokenService(_userManagerMock, jwtOptionsMock, dateTimeProviderMock);
        
        var tokenHandler = new JsonWebTokenHandler();
    
        
        // Act
        string tokenString = await tokenService.GenerateToken(user, Guid.Empty, DateTimeOffset.MinValue);
        
        
        // Assert
        bool isValidJwtFormat = tokenHandler.CanReadToken(tokenString);
        
        Assert.True(isValidJwtFormat);
    }
    
    [Theory]
    [InlineData("TestUser", "test_user@gmail.com", "test-domain.com", "test-domain.com")]
    [InlineData("", "", "test-domain.com", "test-domain.com")]
    public async Task GenerateToken_WithCorrectSecretKey_ReturnsValidToken(string expectedUserName, string expectedEmail, string expectedIssuer, string expectedAudience)
    {
        // Arrange
        var user = new User() { Id = 1, UserName = expectedUserName, Email = expectedEmail };
        
        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        var fakeCreationDate = new DateTime(2024, 11, 24, 06, 24, 45, DateTimeKind.Utc);
        dateTimeProviderMock.UtcNow.Returns(fakeCreationDate);
        
        const string expectedSecret = "a-string-secret-at-least-256-bits-long-used-for-testing";
        const int expectedExpirationMinutes = 15;
        const int expectedExpirationDays = 7;
        
        var jwtOptionsMock = Substitute.For<IOptions<JwtConfig>>();
        var fakeJwtConfig = new JwtConfig()
        {
            Audience = expectedAudience,
            Issuer = expectedIssuer,
            Secret = expectedSecret,
            AccessTokenExpirationMinutes = expectedExpirationMinutes,
            RefreshTokenExpirationDays = expectedExpirationDays
        };
        jwtOptionsMock.Value.Returns(fakeJwtConfig);
    
        ITokenService tokenService = new TokenService(_userManagerMock, jwtOptionsMock, dateTimeProviderMock);
        
        var tokenHandler = new JsonWebTokenHandler();
    
        
        // Act
        string tokenString = await tokenService.GenerateToken(user, Guid.Empty, DateTimeOffset.MinValue);
        
        
        // Assert
        var validationParameters = new TokenValidationParameters()
        {
            ValidIssuer = expectedIssuer,
            ValidAudience = expectedAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(expectedSecret)),
            ValidateLifetime = false // we don't need to check the token's expiration date here, another test already covers that
        };
        var tokenValidationResult = await tokenHandler.ValidateTokenAsync(tokenString, validationParameters);
        
        Assert.True(tokenValidationResult.IsValid);
    }
    
    [Theory]
    [InlineData("TestUser", "test_user@gmail.com", "test-domain.com", "test-domain.com")]
    [InlineData("", "", "test-domain.com", "test-domain.com")]
    public async Task GenerateToken_WithWrongSecretKey_ReturnsInvalidToken(string expectedUserName, string expectedEmail, string expectedIssuer, string expectedAudience)
    {
        // Arrange
        var user = new User() { Id = 1, UserName = expectedUserName, Email = expectedEmail };
        
        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        var fakeCreationDate = new DateTime(2024, 11, 24, 06, 24, 45, DateTimeKind.Utc);
        dateTimeProviderMock.UtcNow.Returns(fakeCreationDate);
        
        const string expectedSecret = "a-string-secret-at-least-256-bits-long-used-for-testing";
        const int expectedExpirationMinutes = 15;
        const int expectedExpirationDays = 7;
        
        var jwtOptionsMock = Substitute.For<IOptions<JwtConfig>>();
        var fakeJwtConfig = new JwtConfig()
        {
            Audience = expectedAudience,
            Issuer = expectedIssuer,
            Secret = expectedSecret,
            AccessTokenExpirationMinutes = expectedExpirationMinutes,
            RefreshTokenExpirationDays = expectedExpirationDays
        };
        jwtOptionsMock.Value.Returns(fakeJwtConfig);
    
        ITokenService tokenService = new TokenService(_userManagerMock, jwtOptionsMock, dateTimeProviderMock);
        
        var tokenHandler = new JsonWebTokenHandler();
    
        
        // Act
        string tokenString = await tokenService.GenerateToken(user, Guid.Empty, DateTimeOffset.MinValue);
        
        
        // Assert
        var validationParameters = new TokenValidationParameters()
        {
            ValidIssuer = expectedIssuer,
            ValidAudience = expectedAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes($"{expectedSecret}-WRONG-SECRET-KEY")),
            ValidateLifetime = false // we don't need to check the token's expiration date here, another test already covers that
        };
        var tokenValidationResult = await tokenHandler.ValidateTokenAsync(tokenString, validationParameters);
        
        Assert.False(tokenValidationResult.IsValid);
    }
}