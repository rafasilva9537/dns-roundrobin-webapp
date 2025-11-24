namespace api.Interfaces.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}