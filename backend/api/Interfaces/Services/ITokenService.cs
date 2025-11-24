using api.Entities;

namespace api.Interfaces.Services;

/// <summary>
/// Provides functionality for generating authentication tokens for users.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the provided user, identifying the user's session and login time.
    /// </summary>
    /// <param name="user">
    /// The user for whom the token is being generated. This should be an instance of the User class containing user details.
    /// </param>
    /// <param name="sessionId">
    /// A unique identifier for the user's session, used to track and differentiate sessions.
    /// </param>
    /// <param name="loginDateUtc">
    /// The UTC date and time of the user's login, marking when the token is issued.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the generated token as a string.
    /// </returns>
    Task<string> GenerateToken(User user, Guid sessionId, DateTimeOffset loginDateUtc);

    /// <summary>
    /// Generates a refresh token that can be used to get a new access JSON Web Token (JWT) without requiring the user's credentials.
    /// </summary>
    /// <returns>
    /// The generated refresh token as a string.
    /// </returns>
    string GenerateRefreshToken();
}