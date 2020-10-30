using Microsoft.Diagnostics.Symbols;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtwEventSource_Test02
{
    class Program
    {
        //public const bool WriteEtwToFile = true;
        public const bool WriteEtwToFile = false;

        static void Main(string[] args)
        {
            int thisProcessId = Process.GetCurrentProcess().Id;
            Console.WriteLine($"Process Id:  {thisProcessId}");

            Console.WriteLine("Starting.");

            var computer = new Computer();
            Task computerTask = Task.Run(computer.Run);

            Dictionary<string, int> stackCounts = new Dictionary<string, int>();
            int totalStackCount = 0;

            int totalProcEventCount = 0;
            int clrProcEventCount = 0;
            int stackwalkProcEventCount = 0;

            Task sessionTask = null;
            TraceEventSession session;
            if (WriteEtwToFile)
            {
                session = new TraceEventSession("MySession", "EtwEventSourceTest02.EventData.etl", TraceEventSessionOptions.Create);
            }
            else
            {
                session = new TraceEventSession("MySession", TraceEventSessionOptions.Create);
            }

            using (session)
            {
                unchecked
                {
                    //session.EnableProvider(ClrRundownTraceEventParser.ProviderGuid,
                    //                        providerLevel:
                    //                                TraceEventLevel.Verbose,
                    //                        matchAnyKeywords:
                    //                                (ulong)ClrRundownTraceEventParser.Keywords.Jit
                    //                                | (ulong)ClrRundownTraceEventParser.Keywords.JittedMethodILToNativeMap
                    //                                | (ulong)ClrRundownTraceEventParser.Keywords.Loader
                    //                                | (ulong)ClrRundownTraceEventParser.Keywords.StartEnumeration
                    //                                | (ulong)ClrRundownTraceEventParser.Keywords.StartEnumeration);

                    session.EnableProvider(ClrRundownTraceEventParser.ProviderGuid,
                                            providerLevel:
                                                    TraceEventLevel.Verbose,
                                            matchAnyKeywords:
                                                    (ulong)ClrRundownTraceEventParser.Keywords.Jit
                                                    | (ulong)ClrRundownTraceEventParser.Keywords.JittedMethodILToNativeMap
                                                    | (ulong)ClrRundownTraceEventParser.Keywords.Loader
                                                    | (ulong)ClrRundownTraceEventParser.Keywords.StartEnumeration
                                                    | (ulong)ClrRundownTraceEventParser.Keywords.Stack
                                                    | (ulong)ClrRundownTraceEventParser.Keywords.CodeSymbolsRundown
                                                    | (ulong)ClrRundownTraceEventParser.Keywords.Compilation);

                    session.EnableProvider(ClrTraceEventParser.ProviderGuid,
                                            providerLevel:
                                                    TraceEventLevel.Verbose,
                                            matchAnyKeywords:
                                                    (ulong)ClrTraceEventParser.Keywords.Stack
                                                    //| (ulong)ClrTraceEventParser.Keywords.Jit
                                                    //| (ulong)ClrTraceEventParser.Keywords.JittedMethodILToNativeMap
                                                    //| (ulong)ClrTraceEventParser.Keywords.StartEnumeration
                                                    //| (ulong)ClrTraceEventParser.Keywords.JITSymbols
                                                    //| (ulong)ClrTraceEventParser.Keywords.Monitoring
                                                    //| (ulong)ClrTraceEventParser.Keywords.Codesymbols
                                                    //| (ulong)ClrTraceEventParser.Keywords.MethodDiagnostic
                                                    //| (ulong)ClrTraceEventParser.Keywords.Compilation

                                                    //| (ulong)ClrTraceEventParser.Keywords.Default
                                                    );
                }

                TraceLogEventSource tleSource = null;
                try
                {
                    if (!WriteEtwToFile)
                    {
                        TextWriter blackHoleSink = new StringWriter();
                        var symbolPath = new SymbolPath(SymbolPath.MicrosoftSymbolServerPath)
                                                .Add(SymbolPath.SymbolPathFromEnvironment)
                                                .Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                        SymbolReader symbolReader = new SymbolReader(Console.Out, symbolPath.ToString());
                        symbolReader.SecurityCheck = (path => true);

                        tleSource = TraceLog.CreateFromTraceEventSession(session);

                        tleSource.Dynamic.All += (e) =>
                        {
                            if (computerTask.IsCompleted)
                            {
                                tleSource.StopProcessing();
                                session.Source.StopProcessing();
                                return;
                            }

                            if (thisProcessId != e.ProcessID)
                            {
                                return;
                            }

                            totalProcEventCount++;

                            if (e.ProviderGuid == ClrTraceEventParser.ProviderGuid)
                            {
                                clrProcEventCount++;
                            }
                            else
                            {
                                //Console.WriteLine($"Non-CLR event: {e.EventName}");
                            }

                            if (!e.EventName.Equals("ClrStack/Walk"))
                            {
                                return;
                            }

                            stackwalkProcEventCount++;

                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine($"Total events: {totalProcEventCount}; CLR events: {clrProcEventCount}; stackwalk events: {stackwalkProcEventCount}; lost events: {tleSource.EventsLost}.");

                            TraceCallStack stack = e.CallStack();
                            if (stack == null)
                            {
                                //Console.WriteLine("CallStack() returned null.");
                                return;
                            }

                            TraceCallStack walkedStack = stack;
                            StringBuilder stackStrBld = new StringBuilder();
                            bool hasGoodFrames = false;
                            while (walkedStack != null)
                            {
                                TraceCodeAddress codeAddress = walkedStack.CodeAddress;
                                if (codeAddress.Method == null)
                                {
                                    TraceModuleFile moduleFile = codeAddress.ModuleFile;
                                    if (moduleFile != null)
                                    {
                                        codeAddress.CodeAddresses.LookupSymbolsForModule(symbolReader, moduleFile);
                                    }
                                    else
                                    {
                                        ;
                                    }
                                }

                                if (stackStrBld.Length > 0)
                                {
                                    stackStrBld.AppendLine();
                                }

                                if (codeAddress.Method != null)
                                {
                                    //stackStrBld.Append(walkedStack.CallStackIndex);
                                    //stackStrBld.Append(": ");
                                    stackStrBld.Append("\"");
                                    stackStrBld.Append(codeAddress.FullMethodName);
                                    stackStrBld.Append("\" in \"");
                                    stackStrBld.Append(codeAddress.ModuleName);
                                    stackStrBld.Append("\"");
                                    hasGoodFrames = true;
                                }
                                else
                                {
                                    stackStrBld.Append("0x");
                                    stackStrBld.Append(codeAddress.Address.ToString("x"));
                                    stackStrBld.Append(" (");
                                    stackStrBld.Append(codeAddress.CodeAddressIndex);
                                    if (! String.IsNullOrEmpty(codeAddress.ModuleName))
                                    {
                                        stackStrBld.Append(") in \"");
                                        stackStrBld.Append(codeAddress.ModuleName);
                                        stackStrBld.Append("\"");
                                        hasGoodFrames = true;
                                    }
                                    else
                                    {
                                        stackStrBld.Append(") in unknown module.");
                                    }
                                }

                                walkedStack = walkedStack.Caller;
                            }

                            if (stackStrBld.Length > 0 && hasGoodFrames == true)
                            {
                                string stackStr = stackStrBld.ToString();
                                int scount;
                                if (stackCounts.TryGetValue(stackStr, out scount))
                                {
                                    scount += 1;
                                }
                                else
                                {
                                    scount = 1;
                                }

                                stackCounts[stackStr] = scount;
                                totalStackCount++;

                                Console.WriteLine($"[{e.TimeStampRelativeMSec} msec] Stack sample on thread {e.ThreadID}."
                                                + $" This stack was seen {scount} times on any thread;"
                                                + $" this is {Math.Round(scount * 100000.0 / totalStackCount) / 1000.0}% of all samples."
                                                + $" Total observed stacks: {totalStackCount}; Total distinct stacks: {stackCounts.Count}.");
                                Console.WriteLine(stackStr);
                            }

                            // Console.WriteLine(stack);
                        };

                        sessionTask = Task.Run(() => session.Source.Process());
                    }

                    Task.Run(() =>
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(5));
                            session.DisableProvider(ClrRundownTraceEventParser.ProviderGuid);
                            Console.WriteLine("*********** Disabled Rundown provider!");
                        });

                    Console.WriteLine("Started.");
                    Console.WriteLine("Press enter to finish.");
                    Console.ReadLine();
                }
                finally
                {
                    if (tleSource != null)
                    {
                        tleSource.Dispose();
                    }
                }
            }

            computer.Stop();
            computerTask.Wait();
            
            if (sessionTask != null)
            {
                sessionTask.Wait();
            }

            Console.WriteLine("Stopped.");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("*********** *********** *********** *********** *********** *********** *********** *********** *********** *********** *********** ***********");
            Console.WriteLine("*********** *********** *********** *********** *********** *********** *********** *********** *********** *********** *********** ***********");
            Console.WriteLine("*********** *********** *********** *********** *********** *********** *********** *********** *********** *********** *********** ***********");
            Console.WriteLine("*********** *********** *********** *********** *********** *********** *********** *********** *********** *********** *********** ***********");
            Console.WriteLine("*********** *********** *********** *********** *********** *********** *********** *********** *********** *********** *********** ***********");
            Console.WriteLine($"Total observed stacks: {totalStackCount}; Total distinct stacks: {stackCounts.Count}.");

            var sortedStackCounts = stackCounts.OrderBy((kvp) => kvp.Value);
            foreach (KeyValuePair<string, int> stackStats in sortedStackCounts)
            {
                Console.WriteLine();
                Console.WriteLine($"Next stack was seen {stackStats.Value} times on any thread;"
                                + $" this is {Math.Round(stackStats.Value * 100000.0 / totalStackCount) / 1000.0}% of all samples.");
                Console.WriteLine(stackStats.Key);
            }

            Console.WriteLine($"Total observed stacks: {totalStackCount}; Total distinct stacks: {stackCounts.Count}.");

            Console.WriteLine("Press enter to terminate.");
            Console.ReadLine();
        }
    }
}
