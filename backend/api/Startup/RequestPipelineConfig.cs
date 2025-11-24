using api.Data;
using api.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace api.Startup;

internal static class RequestPipelineConfig
{
    internal static void UseNonProductionDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        dbContext.Database.Migrate();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
        DatabaseSeeder.SeedRoles(roleManager);
    }

    internal static void UseProductionDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
        DatabaseSeeder.SeedRoles(roleManager);
    }

    internal static void MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "User Management API";
            options.Theme = ScalarTheme.BluePlanet;
            options.DarkMode = true;
            options.DefaultHttpClient = KeyValuePair.Create(ScalarTarget.JavaScript, ScalarClient.Axios);
        });
    }
}