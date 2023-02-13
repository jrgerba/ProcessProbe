using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessProbe.MemoryInterface.Windows;

public class WindowsMemoryInterface : IMemoryInterface
{
    /*
     * PROCESS_QUERY_INFORMATION (0x0400)
     * PROCESS_VM_OPERATION (0x0008)
     * PROCESS_VM_READ (0x0010)
     * PROCESS_VM_WRITE (0x0020)
     */
    private const int DesiredAccess = 0x0020 | 0x0010 | 0x0008 | 0x0400;

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern nint OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("Kernel32.dll")]
    private static extern bool CloseHandle(nint hObject);

    [DllImport("Kernel32.dll")]
    private static extern unsafe bool WriteProcessMemory(nint hProcess, nint lpBaseAddress, byte* lpBuffer, int nSize,
        out int lpNumberOfBytesWritten);

    [DllImport("Kernel32.dll")]
    private static extern unsafe bool ReadProcessMemory(nint hProcess, nint lpBaseAddress, byte* lpBuffer, int nSize,
        out int lpNumberOfBytesRead);

    private readonly Process _proc;
    private readonly nint _handle;

    public bool IsOpen { get; private set; }

    public unsafe int Read(nint address, Span<byte> buffer)
    {
        if (!IsOpen)
            throw new MemoryInterfaceClosedException();

        fixed (byte* bufferPtr = buffer)
        {
            if (!ReadProcessMemory(_handle, address, bufferPtr, buffer.Length, out int bytesRead))
                throw new AccessViolationException("Could not read from selected memory");

            return bytesRead;
        }
    }

    public unsafe int Write(nint address, Span<byte> buffer)
    {
        if (IsOpen) 
            throw new MemoryInterfaceClosedException();

        fixed (byte* bufferPtr = buffer)
        {
            if (!WriteProcessMemory(_handle, address, bufferPtr, buffer.Length, out int bytesWritten))
                throw new AccessViolationException("Could not write to selected memory");

            return bytesWritten;
        }
    }

    public void CloseInterface()
    {
        if (!IsOpen)
            return;

        CloseHandle(_handle);
        _proc.Exited -= OnProcessClosed;

        IsOpen = false;
    }

    private void OnProcessClosed(object? sender, EventArgs args) => CloseInterface();


    public WindowsMemoryInterface(Process proc)
    {
        _proc = proc;
        _handle = OpenProcess(DesiredAccess, false, _proc.Id);

        if (_handle == nint.Zero)
            throw new CreateHandleFailedException(
                $"Handle creation failed with an error code {Marshal.GetLastWin32Error()}");

        _proc.Exited += OnProcessClosed;

        IsOpen = true;
    }

    ~WindowsMemoryInterface()
    {
        CloseInterface();
        _proc.Dispose();
    }
}