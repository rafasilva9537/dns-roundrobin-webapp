namespace api.Endpoints;

internal static class AccountEndpoints
{
    internal static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.MapGroup("/accounts");
    }
}