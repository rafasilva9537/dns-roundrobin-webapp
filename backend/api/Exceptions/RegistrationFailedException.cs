namespace api.Exceptions;

public class RegistrationFailedException : AuthException
{
    public RegistrationFailedException()
    {
    }

    public RegistrationFailedException(string message) : base(message)
    {
    }

    public RegistrationFailedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
