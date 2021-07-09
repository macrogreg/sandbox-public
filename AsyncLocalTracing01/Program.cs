using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Util;

namespace AsyncLocalTracing01
{
    class Program
    {
        static void Main(string[] args)
        {
            (new Program()).Run();
        }

        private static void PrintEnvironmentInformation()
        {
            Process process = Process.GetCurrentProcess();

            Console.WriteLine();
            Console.WriteLine("Environment info { ");
            Console.WriteLine("{ ");
            Console.WriteLine($"    Process Id:             {process.Id}");
            Console.WriteLine($"    Process Name:           {process.ProcessName}");
            Console.WriteLine($"    Current Working Dir:    {Environment.CurrentDirectory}");
            Console.WriteLine($"    Is64BitProcess:         {Environment.Is64BitProcess}");
            Console.WriteLine($"    Runtime version:        {Environment.Version}");
            Console.WriteLine($"    OS version:             {Environment.OSVersion}");
            Console.WriteLine($"    Common App Data folder: {Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}");
            Console.WriteLine();

            Console.WriteLine("    Variables:");
            Console.WriteLine();
            Console.WriteLine("        CORECLR_ENABLE_PROFILING:  " + (Environment.GetEnvironmentVariable("CORECLR_ENABLE_PROFILING") ?? "null"));
            Console.WriteLine("        CORECLR_PROFILER:          " + (Environment.GetEnvironmentVariable("CORECLR_PROFILER") ?? "null"));
            Console.WriteLine("        CORECLR_PROFILER_PATH_64:  " + (Environment.GetEnvironmentVariable("CORECLR_PROFILER_PATH_64") ?? "null"));
            Console.WriteLine("        CORECLR_PROFILER_PATH_32:  " + (Environment.GetEnvironmentVariable("CORECLR_PROFILER_PATH_32") ?? "null"));
            Console.WriteLine("        CORECLR_PROFILER_PATH:     " + (Environment.GetEnvironmentVariable("CORECLR_PROFILER_PATH") ?? "null"));
            Console.WriteLine();
            Console.WriteLine("        COR_ENABLE_PROFILING:      " + (Environment.GetEnvironmentVariable("COR_ENABLE_PROFILING") ?? "null"));
            Console.WriteLine("        COR_PROFILER:              " + (Environment.GetEnvironmentVariable("COR_PROFILER") ?? "null"));
            Console.WriteLine("        COR_PROFILER_PATH_64:      " + (Environment.GetEnvironmentVariable("COR_PROFILER_PATH_64") ?? "null"));
            Console.WriteLine("        COR_PROFILER_PATH_32:      " + (Environment.GetEnvironmentVariable("COR_PROFILER_PATH_32") ?? "null"));
            Console.WriteLine("        COR_PROFILER_PATH:         " + (Environment.GetEnvironmentVariable("COR_PROFILER_PATH") ?? "null"));
            Console.WriteLine();
            Console.WriteLine("        COMPlus_EnableDiagnostics: " + (Environment.GetEnvironmentVariable("COMPlus_EnableDiagnostics") ?? "null"));
            Console.WriteLine();

            Console.WriteLine("    RuntimeEnvironmentInfo:");
            Console.WriteLine();
            Console.WriteLine("        " + RuntimeEnvironmentInfo.SingeltonInstance.ToString());

            Console.WriteLine();
            Console.WriteLine("    AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName:");
            Console.WriteLine("        \"" + (AppDomain.CurrentDomain?.SetupInformation?.TargetFrameworkName ?? "<NULL>") + "\"");

            Console.WriteLine("} ");
            Console.WriteLine();
        }

        // -----------

        private static ThreadLocal<ThreadInfo> s_threadInfo = new ThreadLocal<ThreadInfo>(Program.ThreadInfo.CreateInstance);
        public static ThreadInfo CurrentThreadInfo
        {
            get { return s_threadInfo.Value; }
        }

        private static ConcurrentDictionary<int, List<string>> s_threadHistories = new ConcurrentDictionary<int, List<string>>();

        private AsyncLocal<string> _asyncLocalString = new AsyncLocal<string>(Program.AsyncLocalValueChanged);

        public void Run()
        {
            PrintEnvironmentInformation();

            Console.WriteLine($"Main thread. ThreadId={Thread.CurrentThread.ManagedThreadId}.");

            Task mainTask = Task.Run(RunAsync);
            mainTask.GetAwaiter().GetResult();

            Console.WriteLine("\n\nFinished.");

            foreach(KeyValuePair<int, List<string>> threadHistory in s_threadHistories.OrderBy((th) => th.Key))
            {
                Console.WriteLine($"\nHistory of ThreadId={threadHistory.Key}:");
                foreach (string threadState in threadHistory.Value)
                {
                    Console.WriteLine($"    \"{threadState}\"");
                }
            }
        }

        public async Task RunAsync()
        {
            Console.WriteLine($"Worker initialization step:"
                            + $" ThreadId={Thread.CurrentThread.ManagedThreadId};"
                            + $" ThreadInfoHolderCreated={s_threadInfo.IsValueCreated}.");

            Console.WriteLine($"Worker initialization step:"
                            + $" ThreadId={Thread.CurrentThread.ManagedThreadId};"
                            + $" ThrdInfo.IsInit={CurrentThreadInfo.IsInitialized};"
                            + $" ThrdInfo.State={CurrentThreadInfo.CurrentState ?? "NULL"}.");

            Console.WriteLine($"Worker initialization step:."
                            + $" ThreadId={Thread.CurrentThread.ManagedThreadId};"
                            + $" ThreadInfoHolderCreated={s_threadInfo.IsValueCreated}.");
            
            var workerTasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                workerTasks.Add(DoWork(i));
            }

            await Task.WhenAll(workerTasks);
        }

        public async Task DoWork(int workerId)
        {
            string workerMoniker = $"workerId={workerId}";

            string prevAsyncLocalValue = _asyncLocalString.Value;
            _asyncLocalString.Value = $"AL.{workerMoniker}";

            const string Indent = "    ";
            workerMoniker = $"{Indent}[{workerMoniker}]";
            for (int i = 0; i < workerId; i++)
            {
                workerMoniker = Indent + workerMoniker;
            }

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"\n{workerMoniker} [i={i}]"
                                + $" asLoc=\"{_asyncLocalString.Value}\";"
                                + $" ThrdInfo.State=\"{CurrentThreadInfo.CurrentState}\";"
                                //+ $" ThrdInfo.IsInit={CurrentThreadInfo.IsInitialized};"
                                + $" Before Delay: ThreadId={Thread.CurrentThread.ManagedThreadId}.");

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                Console.WriteLine($"\n{workerMoniker} [i={i}]"
                                + $" asLoc=\"{_asyncLocalString.Value}\";"
                                + $" ThrdInfo.State=\"{CurrentThreadInfo.CurrentState}\";"
                                //+ $" ThrdInfo.IsInit={CurrentThreadInfo.IsInitialized};"
                                + $" After Delay: ThreadId={Thread.CurrentThread.ManagedThreadId}.");
            }

            _asyncLocalString.Value = prevAsyncLocalValue;
        }

        private static void AsyncLocalValueChanged(AsyncLocalValueChangedArgs<string> changeInfo)
        {
            Console.WriteLine($"* AsyncLocalValueChanged."
                            + $" PrevVal=\"{changeInfo.PreviousValue}\","
                            + $" CurrVal=\"{changeInfo.CurrentValue}\","
                            + $" CtxChange={changeInfo.ThreadContextChanged},"
                            + $" ThreadId={Thread.CurrentThread.ManagedThreadId}.");

            CurrentThreadInfo.CurrentState = changeInfo.CurrentValue;
        }

        // -----------

        public class ThreadInfo
        {
            public static ThreadInfo CreateInstance()
            {
                return new ThreadInfo();
            }

            private bool _isInitialized = false;
            private string _currentState = null;

            private ThreadInfo()
            {
                Console.WriteLine($"+ Executing default {nameof(ThreadInfo)} ctor on ThreadId={Thread.CurrentThread.ManagedThreadId}.");
            }

            public bool IsInitialized
            {
                get { return _isInitialized; }
            }

            public string CurrentState
            {
                get { return _currentState; }

                set
                {
                    EnsureInitialized();

                    s_threadHistories.GetOrAdd(Thread.CurrentThread.ManagedThreadId, (_) => new List<string>() ).Add(value ?? "NULL");

                    _currentState = value;
                }
            }

            public void EnsureInitialized()
            {
                _isInitialized = true;
            }
        }
    }
}
