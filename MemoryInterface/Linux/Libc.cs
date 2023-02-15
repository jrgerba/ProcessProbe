using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Linux;

internal static class Libc
{
    [DllImport("libc")]
    public static extern unsafe int process_vm_readv(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov, ulong riovcnt, ulong flags);

    [DllImport("libc")]
    public static extern unsafe int process_vm_writev(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov, ulong riovcnt, ulong flags);
}