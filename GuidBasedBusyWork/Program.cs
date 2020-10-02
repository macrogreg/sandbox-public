using Datadog.Trace.PerfTesting.GuidBasedBusyWork;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace GuidBasedBusyWork
{
    class Program
    {
        static void Main(string[] args)
        {
            (new Program()).Run();
        }

        public void Run()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        public async Task RunAsync()
        {
            var worker = new BusyWorker();
            BusyWorkResult uselessStuff = await worker.DoUselessStuff();

            string json = JsonConvert.SerializeObject(uselessStuff, Formatting.Indented);

            Console.WriteLine();
            Console.WriteLine(json);
            Console.WriteLine();
            Console.WriteLine("Press enter.");
            Console.ReadLine();
        }
    }
}
