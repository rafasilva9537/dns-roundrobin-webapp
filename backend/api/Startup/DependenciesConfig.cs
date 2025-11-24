using System.Diagnostics;
using System.Text;
using api.Configuration;
using api.Constants;
using api.Data;
using api.Entities;
using api.Interfaces.Services;
using api.Middlewares;
using api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.Startup;

internal static class DependenciesConfig
{
    internal static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IDateTimeOffsetProvider, DateTimeOffsetProvider>();
        
        return services;
    }
    
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
            var jwtConfig = configuration.GetSection(JwtConfig.SectionName).Get<JwtConfig>();
            if (jwtConfig is null) 
                throw new InvalidOperationException($"Failed to load {JwtConfig.SectionName} from configuration.");

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
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
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyConstants.AdminOnly, policy =>
            {
                policy.RequireRole(RoleConstants.Admin);
            });
        
        return services;
    }
    
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }
}