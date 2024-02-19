using System.Diagnostics;
using System.Runtime.InteropServices;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace ProcessProbe.MemoryInterface.Linux
{
    internal sealed class LinuxMemoryInterface : IMemoryInterface
    {
        private readonly Process _proc;
        private readonly Dictionary<string, nint> _exportMap;

        public bool IsOpen { get; private set; }

        public unsafe int Read(nint address, Span<byte> buffer)
        {
            if (!IsOpen)
                throw new MemoryInterfaceClosedException();
            
            fixed (void* bufferPtr = buffer)
            {
                iovec local = new() { iov_base = bufferPtr, iov_len = (ulong)buffer.Length };
                iovec remote = new() { iov_base = (void*)address, iov_len = (ulong)buffer.Length };
                return Libc.process_vm_readv(_proc.Id, &local, 1, &remote, 1, 0);
            }
        }

        public unsafe int Write(nint address, Span<byte> buffer)
        {
            if (!IsOpen)
                throw new MemoryInterfaceClosedException();
            
            fixed (void* bufferPtr = buffer)
            {
                iovec local = new() { iov_base = bufferPtr, iov_len = (ulong)buffer.Length };
                iovec remote = new() { iov_base = (void*)address, iov_len = (ulong)buffer.Length };
                return Libc.process_vm_writev(_proc.Id, &local, 1, &remote, 1, 0);
            }
        }

        public nint GetExportedObject(string name) => _exportMap.TryGetValue(name, out nint addr) ? addr : nint.Zero;

        public void CloseInterface()
        {
            IsOpen = false;
        }

        private const string ErrorNoRead = "The binary data could not be read";
        private const string ErrorNoAccess = "Access denied";
        private const string ErrorGeneric = "An unknown error has occured";
        
        public LinuxMemoryInterface(Process proc)
        {
            _proc = proc;
            _exportMap = new Dictionary<string, nint>();
            IsOpen = true;

            proc.Exited += (_, _) =>
            {
                CloseInterface();
            };

            if (_proc.MainModule is null)
                return;

            FileStream stream;

            try
            {
                stream = File.OpenRead($"/proc/{_proc.Id}/exe");
            }
            catch (FileNotFoundException e)
            {
                throw new InterfaceCreationFailedException(ErrorNoRead, e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new InterfaceCreationFailedException(ErrorNoAccess, e);
            }
            catch (Exception e)
            {
                throw new InterfaceCreationFailedException(ErrorGeneric, e);
            }

            if (!ELFReader.TryLoad(stream, false, out IELF elf))
                throw new InterfaceCreationFailedException(ErrorNoRead);

            ISymbolTable symtab;

            if (elf.TryGetSection(".symtab", out ISection sect))
                symtab = (ISymbolTable)sect;
            else
                return;

            foreach (ISymbolEntry sym in symtab.Entries)
            {
                nint addr;

                if (sym is SymbolEntry<uint> i32addr)
                    addr = (nint)(_proc.MainModule.BaseAddress + i32addr.Value);
                else if (sym is SymbolEntry<ulong> i64addr)
                    addr = (nint)((ulong)_proc.MainModule.BaseAddress + i64addr.Value);
                else
                    continue;

                // This is prone to bugs, there should be a way to access identical symbols.
                if (_exportMap.ContainsKey(sym.Name))
                    continue;
                
                _exportMap.Add(sym.Name, addr);
            }

            elf.Dispose();
            stream.Close();
        }

        public void Dispose()
        {
            CloseInterface();
            _proc.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct iovec
    {
        public void* iov_base;
        public ulong iov_len;
    }
}
