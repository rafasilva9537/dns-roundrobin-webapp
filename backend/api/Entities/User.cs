using Microsoft.AspNetCore.Identity;

namespace api.Entities;

public class User : IdentityUser<long>
{
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    // We want to enforce non-null values for the properties below.
    // Ignoring warning is safe here, as we won't be using IdentityUser directly, only derived User class.
    public override string UserName { get; set; } = string.Empty;
    public override string Email { get; set; } = string.Empty;
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}