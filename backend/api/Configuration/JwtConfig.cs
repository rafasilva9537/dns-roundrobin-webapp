using System.ComponentModel.DataAnnotations;

namespace api.Configuration;

public class JwtConfig
{
    public const string SectionName = "JwtConfig";

    [Required]
    public required string Secret { get; init; }
    
    [Required]
    public required string Issuer { get; init; }
    
    [Required]
    public required string Audience { get; init; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public required int AccessTokenExpirationMinutes { get; init; }
    
    [Required]
    [Range(0.5, int.MaxValue)]
    public required int RefreshTokenExpirationDays { get; init; }
}