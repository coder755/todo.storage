namespace todo.storage.model.Exceptions;

public class CreateUserException : Exception
{
    public CreateUserException()
    {
    }

    public CreateUserException(string message)
        : base(message)
    {
    }

    public CreateUserException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}