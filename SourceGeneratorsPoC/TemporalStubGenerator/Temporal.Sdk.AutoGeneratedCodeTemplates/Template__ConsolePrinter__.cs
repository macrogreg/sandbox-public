using System;

namespace Temporal.Sdk.Generated
{
    public class Template__ConsolePrinter__
    {
        private readonly string _prefix;

        public Template__ConsolePrinter__(string prefix)
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
