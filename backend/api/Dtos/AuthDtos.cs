namespace api.Dtos;

public sealed record LoginUserRequest(string Username, string Password);
public sealed record LoginUserResponse(string AccessToken, string RefreshToken);

public sealed record RegisterUserRequest(string Username, string Email, string Password);
public sealed record RegisterUserResponse(string AccessToken, string RefreshToken);