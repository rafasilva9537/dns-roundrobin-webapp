namespace api.Interfaces.Services;

/// <summary>
/// Provides an abstraction for accessing date and time of type <see cref="DateTime"/>.
/// Useful for ensuring consistent time references across the application
/// and for facilitating unit testing by allowing the date and time
/// to be mocked or substituted.
/// </summary>
public interface IDateTimeProvider
{
    /// Gets the current date and time in Coordinated Universal Time (UTC).
    DateTime UtcNow { get; }
}