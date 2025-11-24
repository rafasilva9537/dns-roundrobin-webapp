using api.Data;
using api.Data.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
}