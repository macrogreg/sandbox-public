using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrivialWebInvoker
{
    public class Invoker
    {
        public const int TargetRequestsPerSecond = 50;
        public const int StatsPeriodDurationSecs = 10;

        public const string TragetUrl = "http://localhost:49201/";

        private const int StatsPeriodInvocationsCount = TargetRequestsPerSecond * StatsPeriodDurationSecs;
        private static readonly TimeSpan Period = TimeSpan.FromMilliseconds(1000 / TargetRequestsPerSecond);

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
                DateTimeOffset nextPeriodTargetTime = invokeStart + Period;

                InvokeAsync().GetAwaiter().GetResult();

                DateTimeOffset invokeEnd = DateTimeOffset.Now;
                totalInvocations++;
                statsPeriodInvocations++;

                double durationMillis = (invokeEnd - invokeStart).TotalMilliseconds;
                totalDurationMillisSum += durationMillis;
                statsPeriodDurationMillisSum += durationMillis;

                if (statsPeriodInvocations == StatsPeriodInvocationsCount)
                {
                    TimeSpan statsPeriodRuntime = invokeEnd - statsPeriodStartTime;
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

                TimeSpan sleepPeriod = nextPeriodTargetTime - DateTimeOffset.Now;
                if (sleepPeriod > TimeSpan.Zero)
                {
                    Thread.Sleep(sleepPeriod);
                }

                stopedSignal = _stopedSignal;
            }

            stopedSignal.Set();
        }

        private async Task InvokeAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(TragetUrl);
                    string responseContent = await response.Content.ReadAsStringAsync();
                    int contentLen = responseContent.Length;

                    //Console.Write(contentLen);
                    //Console.Write(" | ");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
