namespace ProcessProbe.MemoryInterface.Windows
{
    public class CreateHandleFailedException : Exception
    {
        public CreateHandleFailedException() : base() { }
        public CreateHandleFailedException(string message) : base(message) { }
    }
}