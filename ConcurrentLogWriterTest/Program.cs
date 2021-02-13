using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentLogWriterTest
{
    class Program
    {
        private const string FileName = "ConcurrentLogWriterTest.txt";
        private const string MutextName = "Global\\ConcurrentLogWriterTest_CD0E3184-10C6-488E-BDFE-D966072E4DF0";

        private string _outputFilePath = null;
        private StreamWriter _logWriter = null;
        private bool _stopRequested = false;
        private Guid _myId;
        private bool _keepLogfileOpen;
        private int _logLineIndex = 0;

        private Mutex _fileMutex = null;

        static void Main(string[] args)
        {
            (new Program()).Exec(args);
        }

        public void Exec(string[] args)
        {
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} started.");

            if (args != null && args.Length > 0 && "-keepOpen".Equals(args[0], StringComparison.OrdinalIgnoreCase))
            {
                _keepLogfileOpen = true;
            }
            else if (args != null && args.Length > 0 && "-alwaysReopen".Equals(args[0], StringComparison.OrdinalIgnoreCase))
            {
                _keepLogfileOpen = false;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("!! WARNING: Usage is ConcurrentLogWriterTest.exe -keepOpen OR ConcurrentLogWriterTest.exe -alwaysReopen");
                Console.WriteLine("Defaulting to: -keepOpen");
                _keepLogfileOpen = true;
            }

            _myId = Guid.NewGuid();

            _outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);

            Console.WriteLine();
            Console.WriteLine($"Starting {nameof(SpinAndWriteLog)}.");
            Console.WriteLine($"    keepLogfileOpen: {_keepLogfileOpen};");
            Console.WriteLine($"    _outputFilePath: \"{_outputFilePath}\";");
            Console.WriteLine($"    _myId: {_myId};");

            Log.Configure.Info(WriteInfoLog);
            _stopRequested = false;
            _fileMutex = new Mutex(initiallyOwned: false, MutextName);
            Task runner = Task.Run(SpinAndWriteLog);

            Console.WriteLine();
            Console.WriteLine("Press Enter to stop.");
            Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("Stopping...");
            _stopRequested = true;

            runner.Wait();

            _fileMutex.Dispose();
            _fileMutex = null;

            Console.WriteLine();
            Console.WriteLine($"_myId: {_myId}. Stopped. Good bye.");
        }

        private async Task SpinAndWriteLog()
        {
            Log.Info(nameof(ConcurrentLogWriterTest), $"STARTING {nameof(SpinAndWriteLog)}", "_myId", _myId, "_keepLogfileOpen", _keepLogfileOpen);

            while (!_stopRequested)
            {
                Log.Info(nameof(ConcurrentLogWriterTest), $"This is a fancy log line", "_myId", _myId, "_logLineIndex", ++_logLineIndex);

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            Log.Info(nameof(ConcurrentLogWriterTest), $"FINISHING {nameof(SpinAndWriteLog)}", "_myId", _myId);
        }

        private void WriteInfoLog(string componentName, string message, params object[] dataNamesAndValues)
        {
            bool ownsMutex = _fileMutex.WaitOne();
            try
            {
                if (_logWriter == null)
                {
                    Stream logStream = new FileStream(_outputFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    _logWriter = new StreamWriter(logStream, Encoding.UTF8, leaveOpen: false);
                }

                _logWriter.BaseStream.Seek(0, SeekOrigin.End);

                StringBuilder logLine = Log.DefaultFormat.ConstructLogLine(Log.DefaultFormat.LogLevelMoniker_Info, componentName, useUtcTimestamp: true, message, dataNamesAndValues);

                _logWriter.WriteLine(logLine.ToString());
                _logWriter.Flush();

                if (!_keepLogfileOpen)
                {
                    _logWriter.Dispose();
                    _logWriter = null;
                }
            }
            finally
            {
                if (ownsMutex)
                {
                    _fileMutex.ReleaseMutex();
                }
            }
        }
    }
}