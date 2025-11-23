using Microsoft.AspNetCore.Identity;

namespace api.Entities;

public class User : IdentityUser<long>
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
    public string Description { get; set; } = string.Empty;
}