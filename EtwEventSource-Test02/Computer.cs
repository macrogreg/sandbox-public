using System;
using System.Threading;
using System.Threading.Tasks;

namespace EtwEventSource_Test02
{
    public class Computer
    {
        public static readonly TimeSpan StatsPeriodDuration = TimeSpan.FromSeconds(5);

        public static readonly TimeSpan InvokeHotLoopDuration = TimeSpan.FromMilliseconds(100);

        private readonly Random _rnd = new Random();
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
            int totalInvocations = 0;
            int statsPeriodInvocations = 0;
            double totalDurationMillisSum = 0.0;
            double statsPeriodDurationMillisSum = 0.0;
            ulong totalInnerIterations = 0;
            ulong statsPeriodInnerIterations = 0;

            DateTimeOffset statsPeriodStartTime, startTime;
            statsPeriodStartTime = startTime = DateTimeOffset.Now;

            ManualResetEventSlim stopedSignal = _stopedSignal;
            while (stopedSignal == null)
            {
                DateTimeOffset invokeStart = DateTimeOffset.Now;
                ulong innerIterations = ComputeAsync().GetAwaiter().GetResult();
                DateTimeOffset invokeEnd = DateTimeOffset.Now;
                totalInvocations++;
                statsPeriodInvocations++;
                totalInnerIterations += innerIterations;
                statsPeriodInnerIterations += innerIterations;

                double durationMillis = (invokeEnd - invokeStart).TotalMilliseconds;
                totalDurationMillisSum += durationMillis;
                statsPeriodDurationMillisSum += durationMillis;

                TimeSpan statsPeriodRuntime = invokeEnd - statsPeriodStartTime;
                if (statsPeriodRuntime >= StatsPeriodDuration)
                {
                    Console.WriteLine();
                    Console.WriteLine("Latest stats period:");
                    Console.WriteLine($"  Invocations:            {statsPeriodInvocations}.");
                    Console.WriteLine($"  Time:                   {statsPeriodRuntime}.");
                    Console.WriteLine($"  Mean invocatons/sec:    {statsPeriodInvocations / (statsPeriodRuntime).TotalSeconds}.");
                    Console.WriteLine($"  Mean lattency:          {statsPeriodDurationMillisSum / statsPeriodInvocations} msecs.");
                    Console.WriteLine($"  Mean inner iterations:  {statsPeriodInnerIterations / (double) statsPeriodInvocations}.");


                    TimeSpan totalRuntime = invokeEnd - startTime;
                    Console.WriteLine("Total:");
                    Console.WriteLine($"  Invocations:            {totalInvocations}.");
                    Console.WriteLine($"  Time:                   {totalRuntime}.");
                    Console.WriteLine($"  Mean invocatons/sec:    {totalInvocations / (totalRuntime).TotalSeconds}.");
                    Console.WriteLine($"  Mean lattency:          {totalDurationMillisSum / totalInvocations} msecs.");
                    Console.WriteLine($"  Mean inner iterations:  {totalInnerIterations / (double)totalInvocations}.");
                    Console.WriteLine();

                    statsPeriodInvocations = 0;
                    statsPeriodDurationMillisSum = 0.0;
                    statsPeriodStartTime = invokeEnd;
                }

                Thread.Yield();

                stopedSignal = _stopedSignal;
            }

            stopedSignal.Set();
        }

        private Task<ulong> ComputeAsync()
        {
            try
            {
                string text = "";
                uint number = 0;

                ulong iterations = 0;

                // Hot spin:
                DateTimeOffset start = DateTimeOffset.Now;
                do
                {
                    ++iterations;

                    uint n = (uint) _rnd.Next();
                    number ^= n;

                    if (text.Length > 0)
                    {
                        text += ", ";
                    }

                    text += $"({number}/{n})";
                }
                while (DateTimeOffset.Now - start < InvokeHotLoopDuration);

                if (text.Length < 1)
                {
                    Console.WriteLine("This will never happen, but now text won't be oplimized away.");
                }

                return Task.FromResult(iterations);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Task.FromResult((ulong) 0);
            }
        }
    }
}
