using api.Entities;

namespace api.Interfaces.Services;

public interface ITokenService
{
    Task<string> GenerateToken(User user);
}