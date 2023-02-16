using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Windows
{
    internal static partial class Kernel32
    {
        [LibraryImport("Kernel32.dll", SetLastError = true)]
        public static partial nint OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [LibraryImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool CloseHandle(nint hObject);

        [LibraryImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte* lpBuffer, int nSize,
            out int lpNumberOfBytesWritten);
        [LibraryImport("Kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static unsafe partial bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, byte* lpBuffer, int nSize,
            out int lpNumberOfBytesRead);

    }
}