namespace ProcessProbe.MemoryInterface;

public class InterfaceCreationFailedException : Exception
{
    public InterfaceCreationFailedException(string message) : base(message)
    {
        
    }

    public InterfaceCreationFailedException(string message, Exception? inner) : base(message, inner)
    {
        
    }
}