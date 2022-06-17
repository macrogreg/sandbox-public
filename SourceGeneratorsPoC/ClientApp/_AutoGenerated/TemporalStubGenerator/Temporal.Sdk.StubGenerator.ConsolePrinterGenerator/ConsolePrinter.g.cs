using System;

namespace Temporal.Sdk.Generated
{
    public class ConsolePrinter
    {
        private readonly string _prefix;

        public ConsolePrinter(string prefix)
        {
            _prefix = prefix ?? String.Empty;
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string msg)
        {
            msg = msg ?? String.Empty;
            Console.WriteLine($"[{_prefix}]!! {msg}");
        }
    }
}
