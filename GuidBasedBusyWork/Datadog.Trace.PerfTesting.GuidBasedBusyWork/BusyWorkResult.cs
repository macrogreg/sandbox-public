using System;

namespace Datadog.Trace.PerfTesting.GuidBasedBusyWork
{
    public class BusyWorkResult
    {
        public string AllGuidsAsBytes { get; internal set; }
        public string[] InputGuids { get; internal set; }
        public string[] RoundTrippedGuids { get; internal set; }
        public string Timestamp { get; internal set; }
    }
}
