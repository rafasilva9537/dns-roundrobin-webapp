namespace api.Exceptions;

public abstract class AuthException : Exception
{
    protected AuthException()
    {
    }

    protected AuthException(string message) : base(message)
    {
    }

    protected AuthException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
