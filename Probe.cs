using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using ProcessProbe.MemoryInterface;
using ProcessProbe.MemoryInterface.Linux;
using ProcessProbe.MemoryInterface.Windows;

namespace ProcessProbe;

public class Probe
{
    // Static //
    private static readonly Dictionary<Type, bool> SafetyLookup = new(); 
    
    // Instance //
    private readonly IMemoryInterface _memory;
    private readonly Process _proc;

    public unsafe int Read<T>(nint address, out T value) where T : unmanaged
    {
        value = default;

        fixed (void* valueAddr = &value)
        {
            Span<byte> buffer = new(valueAddr, sizeof(T));

            return _memory.Read(address, buffer);
        }
    }

    public int Read<T>(nint address, int offset, out T value) where T : unmanaged => Read(address + offset, out value);

    public int Read<T>(string exportName, int offset, out T value) where T : unmanaged =>
        Read(_memory.GetExportedObject(exportName), offset, out value);

    public unsafe int Write<T>(nint address, T value) where T : unmanaged
    {
        Span<byte> buffer = new(&value, sizeof(T));

        return _memory.Write(address, buffer);
    }

    public int Write<T>(nint address, int offset, T value) where T : unmanaged => Write(address + offset, value);

    public int Write<T>(string exportName, int offset, T value) where T : unmanaged =>
        Write(_memory.GetExportedObject(exportName), offset, value);

    public unsafe int ReadArray<T>(nint address, Span<T> array) where T : unmanaged
    {
        fixed (void* bufferPtr = array)
        {
            Span<byte> buffer = new(bufferPtr, sizeof(T) * array.Length);

            return _memory.Read(address, buffer);
        }
    }

    public int ReadArray<T>(nint address, int offset, Span<T> array) where T : unmanaged =>
        ReadArray(address + offset, array);

    public int ReadArray<T>(string exportName, int offset, Span<T> array) where T : unmanaged =>
        ReadArray(_memory.GetExportedObject(exportName) + offset, array);

    public unsafe int WriteArray<T>(nint address, Span<T> array) where T : unmanaged
    {
        fixed (void* bufferPtr = array)
        {
            Span<byte> buffer = new(bufferPtr, sizeof(T) * array.Length);

            return _memory.Write(address, buffer);
        }
    }

    public int WriteArray<T>(nint address, int offset, Span<T> array) where T : unmanaged =>
        WriteArray(address + offset, array);

    public int WriteArray<T>(string exportName, int offset, Span<T> array) where T : unmanaged =>
        WriteArray(_memory.GetExportedObject(exportName), offset, array);

    public Probe(Process proc)
    {
        _proc = proc;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _memory = new WindowsMemoryInterface(_proc);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _memory = new LinuxMemoryInterface(_proc);
        }
        else
        {
            throw new NotSupportedException("The given operating system is not supported");
        }
    }
}