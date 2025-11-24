using System.Security.Claims;
using api.Dtos;
using api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;

namespace api.Endpoints;

internal static class AccountEndpoints
{
    internal static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/accounts");

        endpoints.MapGet("/logged-user", async (UserManager<User> userManager, ClaimsPrincipal claimsPrincipal) =>
        {
            string? userName = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Name);
            if (userName is null) return Results.BadRequest();
            
            var user = await userManager.FindByNameAsync(userName);
            if (user is null) return Results.Unauthorized();

            string? sessionIdClaim = claimsPrincipal.FindFirstValue("session_id");
            string? loginDateClaim = claimsPrincipal.FindFirstValue("login_date");

            _ = Guid.TryParse(sessionIdClaim, out var sessionId);
            _ = DateTimeOffset.TryParse(loginDateClaim, out var loginDate);

            var response = new LoggedUserResponse(
                user.UserName,
                loginDate,
                sessionId,
                System.Net.Dns.GetHostName()
            );

            return Results.Ok(response);
        });
    }
}