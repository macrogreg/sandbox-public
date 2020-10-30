using System;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace EtwEventSourceTest01
{
    public class Eventer
    {
        [EventSource(Name = ProcessStackSampleEventSource.EtwProviderName)]
        public class ProcessStackSampleEventSource : EventSource
        {
            public const string EtwProviderName = "Datadog.ContinuousProfiling.Prototypes.Xyz01";

            public static readonly ProcessStackSampleEventSource Emit = new ProcessStackSampleEventSource();

            [Event(eventId: 1,
                    ActivityOptions = EventActivityOptions.None, 
                    Channel = EventChannel.Operational, 
                    Keywords = EventKeywords.None, 
                    Level = EventLevel.LogAlways,
                    Opcode = EventOpcode.Info,
                    Tags = EventTags.None,
                    Task = EventTask.None,
                    Message = "Test Sample Stack Event")]
            public void SampleStack() { WriteEvent(1); }
        }

        public const int PeriodMillis = 10;

        private ManualResetEventSlim _stopedSignal = null;

        public void Stop()
        {
            ManualResetEventSlim stopedSignal = _stopedSignal;

            if (stopedSignal == null)
            {
                var newSignal = new ManualResetEventSlim(initialState: false);
                ManualResetEventSlim existingSignal = Interlocked.CompareExchange(ref _stopedSignal, newSignal, null);

                if (existingSignal == null)
                {
                    stopedSignal = newSignal;
                }
                else
                {
                    stopedSignal = existingSignal;
                    newSignal.Dispose();
                }
            }

            try
            {
                stopedSignal.Wait();
            }
            catch { }

            stopedSignal.Dispose();
        }

        public void Run()
        {
            if (PeriodMillis < 1 || PeriodMillis > 500)
            {
                throw new InvalidOperationException($"{nameof(PeriodMillis)} must be within [1..500] msecs, but it is {PeriodMillis}");
            }

            DateTimeOffset statsPeriodStartTime, startTime;
            statsPeriodStartTime = startTime = DateTimeOffset.Now;

            ManualResetEventSlim stopedSignal = _stopedSignal;
            while (stopedSignal == null)
            {
                ProcessStackSampleEventSource.Emit.SampleStack();

                long sysMillis = Environment.TickCount64;
                int subsecMillis = (int) (sysMillis % 1000);
                int periodRemainingMillis = PeriodMillis - (subsecMillis % PeriodMillis);
                if (periodRemainingMillis < 1)
                {
                    periodRemainingMillis = PeriodMillis;
                }

                Thread.Sleep(periodRemainingMillis);

                stopedSignal = _stopedSignal;
            }

            stopedSignal.Set();
        }
    }
}
