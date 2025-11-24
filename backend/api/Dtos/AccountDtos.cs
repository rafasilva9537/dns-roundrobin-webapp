namespace api.Dtos;

internal sealed record LoggedUserResponse(string UserName, DateTimeOffset LoginDate, Guid SessionId, string GetHostName);