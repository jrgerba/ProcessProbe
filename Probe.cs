using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ProcessProbe.MemoryInterface;
using ProcessProbe.MemoryInterface.Linux;
using ProcessProbe.MemoryInterface.Windows;

namespace ProcessProbe
{
    public class Probe
    {
        private static readonly Dictionary<Type, bool> SafetyLookup = new();

        private readonly IMemoryInterface _memory;

        public Probe(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _memory = new WindowsMemoryInterface(process);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _memory = new LinuxMemoryInterface(process);
            }
            else
            {
                throw new PlatformNotSupportedException("The given operating system is not supported.");
            }
        }

        ~Probe()
        {
            _memory.CloseInterface();
        }

        public int Read<T>(nint address, out T value) where T : unmanaged
        {
            value = default;

            Span<byte> buffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));

            return _memory.Read(address, buffer);
        }

        public int Read<T>(nint address, int offset, out T value) where T : unmanaged => Read(address + offset, out value);

        public int Read<T>(string exportName, int offset, out T value) where T : unmanaged => 
            Read(_memory.GetExportedObject(exportName), offset, out value);

        public int Write<T>(nint address, T value) where T : unmanaged
        {
            Span<byte> buffer = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));

            return _memory.Write(address, buffer);
        }

        public int Write<T>(nint address, int offset, T value) where T : unmanaged => Write(address + offset, value);

        public int Write<T>(string exportName, int offset, T value) where T : unmanaged => 
            Write(_memory.GetExportedObject(exportName), offset, value);

        public int ReadArray<T>(nint address, Span<T> array) where T : unmanaged
        {
            Span<byte> buffer = MemoryMarshal.AsBytes(array);

            return _memory.Read(address, buffer);
        }

        public int ReadArray<T>(nint address, int offset, Span<T> array) where T : unmanaged => 
            ReadArray(address + offset, array);

        public int ReadArray<T>(string exportName, int offset, Span<T> array) where T : unmanaged => 
            ReadArray(_memory.GetExportedObject(exportName) + offset, array);

        public int WriteArray<T>(nint address, Span<T> array) where T : unmanaged
        {
            Span<byte> buffer = MemoryMarshal.AsBytes(array);

            return _memory.Write(address, buffer);
        }

        public int WriteArray<T>(nint address, int offset, Span<T> array) where T : unmanaged => 
            WriteArray(address + offset, array);

        public int WriteArray<T>(string exportName, int offset, Span<T> array) where T : unmanaged => 
            WriteArray(_memory.GetExportedObject(exportName), offset, array);
    }
}
