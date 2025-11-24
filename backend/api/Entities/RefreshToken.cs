namespace api.Entities;

internal class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresOnUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public long UserId { get; set; }

    public User User { get; set; } = null!;
}