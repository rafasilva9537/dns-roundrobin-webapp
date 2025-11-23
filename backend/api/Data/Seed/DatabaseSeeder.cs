using System.Reflection;
using api.Constants;
using Microsoft.AspNetCore.Identity;

namespace api.Data.Seed;

internal static class DatabaseSeeder
{
    internal static void SeedRoles(RoleManager<IdentityRole<long>> roleManager)
    {
        var roleFields = typeof(RoleConstants).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var roleField in roleFields)
        {
            string roleName = roleField.Name;
            
            var role = roleManager.FindByNameAsync(roleName).Result;
            if (role is null)
            {
                var newRole = new IdentityRole<long>(roleName);
                roleManager.CreateAsync(newRole).Wait();
            }
        }
    }
}