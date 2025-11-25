using System.Security.Claims;
using api.Dtos;
using api.Entities;
using Microsoft.AspNetCore.Identity;

namespace api.Endpoints;

internal static class AccountEndpoints
{
    internal static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {

        var endpoints = app.MapGroup("/accounts")
            .RequireAuthorization();

        endpoints.MapGet("/logged-user", async (UserManager<User> userManager, ClaimsPrincipal claimsPrincipal) =>
        {
            string? userName = claimsPrincipal.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Results.Unauthorized();
            }
        
            var user = await userManager.FindByNameAsync(userName);
            if (user is null ||
                !Guid.TryParse(claimsPrincipal.FindFirstValue("session_id"), out var sessionId) ||
                !DateTimeOffset.TryParse(claimsPrincipal.FindFirstValue("login_date"), out var loginDate)) 
            {
                return Results.Unauthorized();
            }

            var response = new LoggedUserResponse(
                user.UserName!,
                loginDate,
                sessionId,
                System.Net.Dns.GetHostName()
            );

            return Results.Ok(response);
        });
    }
}