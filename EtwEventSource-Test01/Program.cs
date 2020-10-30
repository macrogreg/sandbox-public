using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwEventSourceTest01
{
    class Program
    {
        //public const bool CollectEtw = false;
        public const bool CollectEtw = true;
        public const bool PrintEtw = true;
        public const bool ForceAdmin = false;

        private static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool EnsureAdminOrFork()
        {
            if (IsRunAsAdmin())
            {
                Console.WriteLine("This program has been started with administrator priviledges.");
                return true;
            }

            Console.WriteLine("This program must be run as an administrator!");
            Console.WriteLine("Relaunching...");
            Console.WriteLine();

            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.StartInfo.FileName = Path.ChangeExtension(Assembly.GetEntryAssembly().CodeBase, "exe");
            process.StartInfo.Verb = "runas";

            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        private static string nn(string s)
        {
            const string NullStr = "null";
            return s ?? NullStr;
        }

        static void Main(string[] args)
        {
            int thisProcessId = Process.GetCurrentProcess().Id;
            Console.WriteLine($"Process Id:  {thisProcessId}");

            if (CollectEtw && ForceAdmin && !EnsureAdminOrFork())
            {
                return;
            }

            Console.WriteLine("Starting.");

            var eventer = new Eventer();
            Task eventerTask = Task.Run(eventer.Run);

            var invoker = new Invoker();
            Task invokerTask = Task.Run(invoker.Run);

            Task sessionTask = null;

            var knownThreads = new HashSet<int>();

            if (CollectEtw)
            {
                TraceEventSession session = null;
                if (PrintEtw)
                {
                    session = new TraceEventSession("MySession", TraceEventSessionOptions.Create);
                }
                else
                {
                    session = new TraceEventSession("MySession", "EtwEventSourceTest01.EventData.etl", TraceEventSessionOptions.Create);
                }

                using (session)
                {
                    if (TraceEventSession.IsElevated() == true)
                    {
                        Console.WriteLine("TraceEventSession IS elevated.");
                        session.EnableKernelProvider(flags: KernelTraceEventParser.Keywords.All, stackCapture: KernelTraceEventParser.Keywords.All);
                    }
                    else
                    {
                        Console.WriteLine("TraceEventSession is NOT elevated.");
                    }

                    session.EnableProvider(Eventer.ProcessStackSampleEventSource.EtwProviderName);
                    session.EnableProvider(Guid.Parse("A669021C-C450-4609-A035-5AF59AF4DF18"));  // CLR Rundown events
                    session.EnableProvider(Guid.Parse("e13c0d23-ccbc-4e12-931b-d9cc2eee27e4"));  // CLR Runtime events
                    session.EnableProvider("Microsoft-DotNETCore-SampleProfiler");

                    session.Source.Dynamic.All += (e) =>
                        {
                            if (e == null)
                            {
                                return;
                            }

                            if (e.ProviderName.Equals("Microsoft-Windows-DotNETRuntimeRundown")
                                    || e.ProviderName.Equals("MSNT_SystemTrace"))
                            {
                                //Console.WriteLine();
                                //Console.WriteLine();
                                //Console.WriteLine();
                                //Console.WriteLine($"ProviderName:       {nn(e.ProviderName)}");

                                return;
                            }

                            bool mentionsStack = e.EventName.Contains("stack", StringComparison.OrdinalIgnoreCase);

                            if (! mentionsStack && e.PayloadNames != null)
                            {
                                for (int i = 0; !mentionsStack && i < e.PayloadNames.Length; i++)
                                {
                                    if (e.PayloadNames[i].Contains("stack", StringComparison.OrdinalIgnoreCase))
                                    {
                                        mentionsStack = true;
                                    }
                                }
                            }

                            if (! mentionsStack)
                            {
                                return;
                            }

                            //Console.WriteLine($"ProcessName:        {e.ProcessID}");

                            //return;

                            bool thisProcStackWalk = e.ProviderName.Equals("Microsoft-Windows-DotNETRuntime") && e.EventName.Equals("ClrStack/Walk") && e.ProcessID == thisProcessId;

                            if (thisProcStackWalk)
                            {
                                //Console.WriteLine($"ThreadID:           {e.ThreadID}");
                                //Console.WriteLine($"Current ThreadID:   {Thread.CurrentThread.ManagedThreadId}");

                                if (knownThreads.Add(e.ThreadID))
                                {
                                    var threadsStr = new StringBuilder($"Known threads ({knownThreads.Count}):");
                                    foreach(int tId in knownThreads)
                                    {
                                        if (threadsStr.Length > 0)
                                        {
                                            threadsStr.Append(", ");

                                        }

                                        threadsStr.Append(tId);
                                    }

                                    Console.WriteLine(threadsStr);
                                }
                            }

                            //return;
                            // 1073741824

                            if (!thisProcStackWalk)
                            {
                                return;
                            }

                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine($"ProviderName:       {nn(e.ProviderName)}");
                            Console.WriteLine($"EventName:          {nn(e.EventName)}");
                            Console.WriteLine($"Keywords:           {e.Keywords}");
                            Console.WriteLine($"Level:              {e.Level}");
                            Console.WriteLine($"TimeStamp:          {e.TimeStamp}");
                            Console.WriteLine($"ProcessID:          {e.ProcessID}");
                            Console.WriteLine($"ProcessName:        {nn(e.ProcessName)}");
                            Console.WriteLine($"ProcessorNumber:    {e.ProcessorNumber}");
                            Console.WriteLine($"IsClassicProvider:  {e.IsClassicProvider}");
                            Console.WriteLine($"ThreadID:           {e.ThreadID}");

                            if (e.PayloadNames != null)
                            {
                                var payloadNamesStr = new StringBuilder();
                                for (int i = 0; i < e.PayloadNames.Length; i++)
                                {
                                    if (payloadNamesStr.Length > 0)
                                    {
                                        payloadNamesStr.Append(", ");

                                    }

                                    payloadNamesStr.Append(e.PayloadNames[i]);
                                }

                                Console.WriteLine($"PayloadNames:       {payloadNamesStr}");
                            }

                            object stackObj = e.PayloadByName("Stack");
                            if (stackObj != null)
                            {
                                Console.WriteLine($"Stack:              {stackObj}");
                                Console.WriteLine();
                                Console.WriteLine(e.Dump(includePrettyPrint: true, truncateDump: false));
                            }
                        };

                    sessionTask = Task.Run(() => session.Source.Process());

                    Console.WriteLine("Started.");
                    Console.WriteLine("Press enter to finish.");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Started.");
                Console.WriteLine("Press enter to finish.");
                Console.ReadLine();
            }

            if (sessionTask != null)
            {
                sessionTask.GetAwaiter().GetResult();
            }

            invoker.Stop();
            invokerTask.GetAwaiter().GetResult();

            eventer.Stop();
            eventerTask.GetAwaiter().GetResult();

            Console.WriteLine("Stopped.");
            Console.WriteLine("Press enter to terminate.");
            Console.ReadLine();
        }
    }
}
