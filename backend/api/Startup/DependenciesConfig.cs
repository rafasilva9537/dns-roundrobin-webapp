using System.Text;
using api.Configuration;
using api.Data;
using api.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.Startup;

internal static class DependenciesConfig
{
    internal static IServiceCollection AddAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddSqlServer<AppDbContext>(connectionString);
        return services;
    }

    internal static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtConfigOptions = configuration.GetSection(JwtConfigOptions.SectionName).Get<JwtConfigOptions>();
            if (jwtConfigOptions is null) 
                throw new InvalidOperationException($"Failed to load {JwtConfigOptions.SectionName} from configuration.");

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = jwtConfigOptions.Issuer,
                ValidAudience = jwtConfigOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfigOptions.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });
        
        return services;
    }

    internal static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<AppDbContext>();
        
        // TODO: change to more secure configs after
        services.Configure<IdentityOptions>(options => {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
        });
        
        return services;
    }

    internal static IServiceCollection AddPolicyBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder();
        return services;
    }
}