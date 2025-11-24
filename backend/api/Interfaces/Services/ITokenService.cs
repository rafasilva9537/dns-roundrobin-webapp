using api.Entities;

namespace api.Interfaces.Services;

/// <summary>
/// Provides functionality for generating authentication tokens for users.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for the specified user, embedding user claims and other token-related properties.
    /// </summary>
    /// <param name="user">The user for whom the token is being generated. This includes user information such as username and email.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the generated JWT as a string.
    /// </returns>
    Task<string> GenerateToken(User user);
}