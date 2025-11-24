namespace api.Interfaces.Services;

public interface IDateTimeOffsetProvider
{
    DateTimeOffset UtcNow { get; }
}