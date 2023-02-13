namespace ProcessProbe;

public class UnsafeTypeException : Exception
{
    public UnsafeTypeException() : base("The given type is unsafe for the operation")
    {
    }

    public UnsafeTypeException(string? message) : base(message)
    {
    }

    
}