using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Linux;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct iovec
{
    public void* iov_base;
    public int iov_len;
}