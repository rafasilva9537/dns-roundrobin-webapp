namespace api.Exceptions;

public class InvalidRefreshTokenException : AuthException
{
    public InvalidRefreshTokenException()
    {
    }

    public InvalidRefreshTokenException(string message) : base(message)
    {
    }

    public InvalidRefreshTokenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
