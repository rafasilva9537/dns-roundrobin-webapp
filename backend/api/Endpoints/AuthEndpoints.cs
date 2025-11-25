using api.Dtos;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints;

internal static class AuthEndpoints
{
    internal static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/auth");

        endpoints.MapPost("/register", async ([FromBody] RegisterUserRequest request, [FromServices] IAuthService authService) =>
        {
            var response = await authService.RegisterAsync(request);
            return Results.Ok(response);
        });

        endpoints.MapPost("/login", async ([FromBody] LoginUserRequest request, [FromServices] IAuthService authService) =>
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        });

        endpoints.MapPost("/refresh", async ([FromBody] RefreshTokenRequest request, [FromServices] IAuthService authService) =>
        {
            var response = await authService.RefreshAsync(request);
            return Results.Ok(response);
        });
        
        endpoints.MapPost("/logout", async ([FromBody] RefreshTokenRequest request, [FromServices] IAuthService authService) =>
        {
            await authService.LogoutAsync(request.RefreshToken);
            return Results.NoContent();
        });
    }
}