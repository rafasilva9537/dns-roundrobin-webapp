using api.Dtos;

namespace api.Interfaces.Services;

public interface IAuthService
{
    Task<LoginUserResponse> LoginAsync(LoginUserRequest userRequest);
    Task LogoutAsync(string refreshToken);
    Task<RegisterUserResponse> RegisterAsync(RegisterUserRequest userRequest);
    Task<RefreshTokenResponse> RefreshAsync(RefreshTokenRequest request);
}