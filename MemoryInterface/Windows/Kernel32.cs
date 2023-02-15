using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Windows
{
    internal static class Kernel32
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("Kernel32.dll")]
        public static extern bool CloseHandle(nint hObject);

        [DllImport("Kernel32.dll")]
        public static extern unsafe bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte* lpBuffer, int nSize,
            out int lpNumberOfBytesWritten);
        [DllImport("Kernel32.dll")]
        public static extern unsafe bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, byte* lpBuffer, int nSize,
            out int lpNumberOfBytesRead);

    }
}