namespace ProcessProbe.MemoryInterface;

public interface IMemoryInterface
{
    public int Read(nint address, Span<byte> buffer);

    public int Write(nint address, Span<byte> buffer);

    public void CloseInterface();

    public bool IsOpen { get; }
}