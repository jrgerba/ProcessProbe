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
    private static Dictionary<Type, bool> _safetyLookup = new(); 

    private static void EnforceTypeSafety<T>() => EnforceTypeSafety(typeof(T));

    private static void EnforceTypeSafety(Type t)
    {
        if (_safetyLookup.TryGetValue(t, out bool isSafe))
        {
            if (isSafe)
                return;

            throw new UnsafeTypeException("The given type cannot be a reference");
        }

        if (t.IsPrimitive)
            return;

        if (t.IsClass)
            throw new UnsafeTypeException("The given type cannot be a reference");

        FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo f = fields[i];
            EnforceTypeSafety(f.FieldType);
        }

        _safetyLookup.Add(t, true);
    }

    // Instance //

    private readonly IMemoryInterface _memory;
    private readonly Process _proc;

    public unsafe int Read<T>(nint address, out T value) where T : struct
    {
        EnforceTypeSafety<T>();

        value = default;

        fixed (void* valueAddr = &value)
        {
            Span<byte> buffer = new(valueAddr, SizeOf<T>.Size);

            return _memory.Read(address, buffer);
        }
    }

    public int Read<T>(nint address, int offset, out T value) where T : struct => Read(address + offset, out value);

    public int Read<T>(string exportName, int offset, out T value) where T : struct =>
        Read(_memory.GetExportedObject(exportName), offset, out value);

    public unsafe int Write<T>(nint address, T value) where T : struct
    {
        EnforceTypeSafety<T>();

        Span<byte> buffer = new(&value, SizeOf<T>.Size);

        return _memory.Write(address, buffer);
    }

    public int Write<T>(nint address, int offset, T value) where T : struct => Write(address + offset, value);

    public int Write<T>(string exportName, int offset, T value) where T : struct =>
        Write(_memory.GetExportedObject(exportName), offset, value);

    public unsafe int ReadArray<T>(nint address, Span<T> array) where T : struct
    {
        EnforceTypeSafety<T>();

        fixed (void* bufferPtr = array)
        {
            Span<byte> buffer = new(bufferPtr, SizeOf<T>.Size * array.Length);

            return _memory.Read(address, buffer);
        }
    }

    public int ReadArray<T>(nint address, int offset, Span<T> array) where T : struct =>
        ReadArray(address + offset, array);

    public int ReadArray<T>(string exportName, int offset, Span<T> array) where T : struct =>
        ReadArray(_memory.GetExportedObject(exportName) + offset, array);

    public unsafe int WriteArray<T>(nint address, Span<T> array) where T : struct
    {
        EnforceTypeSafety<T>();

        fixed (void* bufferPtr = array)
        {
            Span<byte> buffer = new(bufferPtr, SizeOf<T>.Size * array.Length);

            return _memory.Write(address, buffer);
        }
    }

    public int WriteArray<T>(nint address, int offset, Span<T> array) where T : struct =>
        WriteArray(address + offset, array);

    public int WriteArray<T>(string exportName, int offset, Span<T> array) where T : struct =>
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