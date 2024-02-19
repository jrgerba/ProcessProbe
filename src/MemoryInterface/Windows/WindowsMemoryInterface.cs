using System.Diagnostics;
using System.Runtime.InteropServices;
using PeNet;
using PeNet.Header.Pe;

namespace ProcessProbe.MemoryInterface.Windows
{
    internal sealed class WindowsMemoryInterface : IMemoryInterface
    {
        private static readonly int DesiredAccess = AccessRights.ProcesVmWrite.Value() 
                                           | AccessRights.ProcessVmRead.Value() 
                                           | AccessRights.ProcessVmOperation.Value() 
                                           | AccessRights.ProcessQueryInformation.Value();
        private readonly Process _process;
        private readonly Dictionary<string, nint> _exportMap;
        private readonly nint _handle;

        public bool IsOpen { get; private set; }

        public unsafe int Read(nint address, Span<byte> buffer)
        {
            if (!IsOpen)
                throw new MemoryInterfaceClosedException();

            fixed (byte* bufferPtr = buffer)
            {
                if (!Kernel32.ReadProcessMemory(_handle, address, bufferPtr, buffer.Length, out int bytesRead))
                    throw new AccessViolationException($"Could not read from selected memory ({Marshal.GetLastWin32Error()})");

                return bytesRead;
            }
        }

        public unsafe int Write(nint address, Span<byte> buffer)
        {
            if (!IsOpen) 
                throw new MemoryInterfaceClosedException();

            fixed (byte* bufferPtr = buffer)
            {
                if (!Kernel32.WriteProcessMemory(_handle, address, bufferPtr, buffer.Length, out int bytesWritten))
                    throw new AccessViolationException("Could not write to selected memory");

                return bytesWritten;
            }
        }

        public nint GetExportedObject(string name) => _exportMap.TryGetValue(name, out nint addr) ? addr : nint.Zero;

        public void CloseInterface()
        {
            if (!IsOpen)
                return;

            Kernel32.CloseHandle(_handle);
            _process.Exited -= OnProcessClosed;

            IsOpen = false;
        }

        private void OnProcessClosed(object? sender, EventArgs args) => CloseInterface();

        private const string HandleFailError = "Handle creation failed with an error code {0}";
        private const string MainModuleNullError = "The main module could not be found";

        public WindowsMemoryInterface(Process process)
        {
            _process = process;
            _exportMap = new Dictionary<string, nint>();
            _handle = Kernel32.OpenProcess(DesiredAccess, false, _process.Id);

            if (_handle == nint.Zero)
                throw new InterfaceCreationFailedException(String.Format(HandleFailError, Marshal.GetLastWin32Error()));

            _process.Exited += OnProcessClosed;

            IsOpen = true;

            if (_process.MainModule is not null)
            {
                PeFile header = new(_process.MainModule.FileName);
                ExportFunction[]? exports = header.ExportedFunctions;

                if (exports is not null)
                {
                    int unnamedExports = 0;

                    foreach (ExportFunction export in exports)
                    {
                        string exportName = export.Name ?? $"UnnamedExport_{unnamedExports++:3}";
                        nint exportAddress = (nint)(_process.MainModule.BaseAddress + export.Address);

                        _exportMap.Add(exportName, exportAddress);
                    }
                }
            }
            else
                throw new InterfaceCreationFailedException(MainModuleNullError);
        }

        public void Dispose()
        {
            CloseInterface();
            _process.Dispose();
        }
    }
}
