using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Linux
{
    internal class LinuxMemoryInterface : IMemoryInterface
    {
        private readonly Process _proc;

        public bool IsOpen { get; private set; }

        public unsafe int Read(nint address, Span<byte> buffer)
        {
            fixed (void* bufferPtr = buffer)
            {
                iovec local = new() { iov_base = bufferPtr, iov_len = (ulong)buffer.Length };
                iovec remote = new() { iov_base = (void*)address, iov_len = (ulong)buffer.Length };
                return Libc.process_vm_readv(_proc.Id, &local, 1, &remote, 1, 0);
            }
        }

        public unsafe int Write(nint address, Span<byte> buffer)
        {
            fixed (void* bufferPtr = buffer)
            {
                iovec local = new() { iov_base = bufferPtr, iov_len = (ulong)buffer.Length };
                iovec remote = new() { iov_base = (void*)address, iov_len = (ulong)buffer.Length };
                return Libc.process_vm_writev(_proc.Id, &local, 1, &remote, 1, 0);
            }
        }

        public nint GetExportedObject(string name) => throw new NotImplementedException();

        public void CloseInterface()
        {
            IsOpen = false;
        }

        public LinuxMemoryInterface(Process proc)
        {
            _proc = proc;
            IsOpen = true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct iovec
    {
        public void* iov_base;
        public ulong iov_len;
    }
}
