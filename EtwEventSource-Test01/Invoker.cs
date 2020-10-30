using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwEventSourceTest01
{
    public class Invoker
    {
        public static readonly TimeSpan StatsPeriodDuration = TimeSpan.FromSeconds(3);

        public static readonly TimeSpan InvokeHotLoopDuration = TimeSpan.FromMilliseconds(50);

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

            DateTimeOffset statsPeriodStartTime, startTime;
            statsPeriodStartTime = startTime = DateTimeOffset.Now;

            ManualResetEventSlim stopedSignal = _stopedSignal;
            while (stopedSignal == null)
            {
                DateTimeOffset invokeStart = DateTimeOffset.Now;
                InvokeAsync().GetAwaiter().GetResult();
                DateTimeOffset invokeEnd = DateTimeOffset.Now;
                totalInvocations++;
                statsPeriodInvocations++;

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

                    TimeSpan totalRuntime = invokeEnd - startTime;
                    Console.WriteLine("Total:");
                    Console.WriteLine($"  Invocations:            {totalInvocations}.");
                    Console.WriteLine($"  Time:                   {totalRuntime}.");
                    Console.WriteLine($"  Mean invocatons/sec:    {totalInvocations / (totalRuntime).TotalSeconds}.");
                    Console.WriteLine($"  Mean lattency:          {totalDurationMillisSum / totalInvocations} msecs.");
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

        private Task<int> InvokeAsync()
        {
            try
            {
                int val = _rnd.Next();

                // Hot spin:
                DateTimeOffset start = DateTimeOffset.Now;
                do
                {
                    int n = _rnd.Next();
                    val ^= n;
                }
                while (DateTimeOffset.Now - start < InvokeHotLoopDuration);

                return Task.FromResult(val);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Task.FromResult(0);
            }
        }
    }
}
