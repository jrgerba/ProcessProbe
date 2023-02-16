namespace ProcessProbe.MemoryInterface
{
    public class MemoryInterfaceClosedException : Exception
    {
        public MemoryInterfaceClosedException() : base("The given memory interface has been closed") { }
        public MemoryInterfaceClosedException(string message) : base(message) { }
    }
}