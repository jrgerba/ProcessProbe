using System.Diagnostics;

namespace ProcessProbe
{
    public static class ProcessExtensions
    {
        public static Probe CreateProbe(this Process proc) => new Probe(proc);
    }
}