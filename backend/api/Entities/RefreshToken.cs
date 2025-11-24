namespace api.Entities;

internal class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOnUtc { get; set; }
    public Guid UserPublicId { get; set; }
}