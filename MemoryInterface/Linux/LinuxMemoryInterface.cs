using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Linux;

public class LinuxMemoryInterface : IMemoryInterface
{
    [DllImport("libc")]
    private static extern unsafe int process_vm_readv(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov,
        ulong riovcnt, ulong flags);
    
    [DllImport("libc")]
    private static extern unsafe int process_vm_writev(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov,
        ulong riovcnt, ulong flags);

    private Process _proc;

    public bool IsOpen { get; private set; }

    public unsafe int Read(nint address, Span<byte> buffer)
    {
        fixed (void* bufferPtr = buffer)
        {
            iovec local = new()
            {
                iov_base = bufferPtr,
                iov_len = buffer.Length
            };

            iovec remote = new()
            {
                iov_base = (void*)address,
                iov_len = buffer.Length
            };

            return process_vm_readv(_proc.Id, &local, 1, &remote, 1, 0);
        }
    }

    public unsafe int Write(nint address, Span<byte> buffer)
    {
        fixed (void* bufferPtr = buffer)
        {
            iovec local = new()
            {
                iov_base = bufferPtr,
                iov_len = buffer.Length
            };

            iovec remote = new()
            {
                iov_base = (void*)address,
                iov_len = buffer.Length
            };

            return process_vm_writev(_proc.Id, &local, 1, &remote, 1, 0);
        }
    }

    public nint GetExportedObject(string name)
    {
        throw new NotImplementedException();
    }

    public void CloseInterface()
    {
        IsOpen = false;

        _proc.Dispose();
    }

    public LinuxMemoryInterface(Process proc)
    {
        _proc = proc;
        IsOpen = true;
    }
}