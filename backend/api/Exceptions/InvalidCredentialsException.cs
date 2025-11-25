namespace api.Exceptions;

public class InvalidCredentialsException : AuthException
{
    public InvalidCredentialsException()
    {
    }

    public InvalidCredentialsException(string message) : base(message)
    {
    }

    public InvalidCredentialsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
