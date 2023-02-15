using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Linux;

internal static partial class Libc
{
    [LibraryImport("libc")]
    public static unsafe partial int process_vm_readv(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov, ulong riovcnt, ulong flags);

    [LibraryImport("libc")]
    public static unsafe partial int process_vm_writev(int pid, iovec* local_iov, ulong liovcnt, iovec* remote_iov, ulong riovcnt, ulong flags);
}