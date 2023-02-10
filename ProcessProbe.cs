using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ProcessProbe.MemoryInterface;

namespace ProcessProbe;

public class ProcessProbe
{
    private readonly IMemoryInterface _memory;
    private readonly Process _proc;

    public unsafe int Read<T>(nint address, out T value) where T : struct
    {
        EnforceTypeSafety<T>();

        value = default;

        Span<byte> buffer = new(&value, SizeOf<T>.Size);

        return _memory.Read(address, buffer);
    }

    public unsafe int Write<T>(nint address, out T value) where T : struct
    {
        EnforceTypeSafety<T>();

        Span<byte> buffer = new(&value, SizeOf<T>.Size);

        return _memory.Write(address, buffer);
    }

    public unsafe int ReadArray<T>(nint address, Span<T> array) where T : struct
    {
        EnforceTypeSafety<T>();

        fixed (void* bufferPtr = &array.GetPinnableReference())
        {
            Span<byte> buffer = new(bufferPtr, SizeOf<T>.Size * array.Length);

            return _memory.Read(address, buffer);
        }
    }

    public unsafe int WriteArray<T>(nint address, Span<T> array) where T : struct
    {
        EnforceTypeSafety<T>();

        fixed (void* bufferPtr = &array.GetPinnableReference())
        {
            Span<byte> buffer = new(bufferPtr, SizeOf<T>.Size * array.Length);

            return _memory.Write(address, buffer);
        }
    }

    private void EnforceTypeSafety<T>() { }
}