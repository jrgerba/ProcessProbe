namespace ProcessProbe.MemoryInterface.Windows;

public enum AccessRights
{
    // https://learn.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights
    
    ProcessVmRead = 0x0010,
    ProcesVmWrite = 0x0020,
    ProcessVmOperation = 0x0008,
    ProcessQueryInformation = 0x0400,
}

public static class AccessRightsHelper
{
    public static int Value(this AccessRights accessRight)
        => (int)accessRight;
}