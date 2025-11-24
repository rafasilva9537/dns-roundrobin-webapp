namespace api.Dtos;

internal sealed record LoggedUserResponse(string UserUserName, DateTimeOffset LoginDate, Guid SessionId, string GetHostName);