using System;
using System.Threading.Tasks;

namespace TrivialWebInvoker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting.");

            var invoker = new Invoker();
            Task invokerTask = Task.Run(invoker.Run);

            Console.WriteLine("Started.");
            Console.WriteLine("Press enter to finish.");
            Console.ReadLine();

            invoker.Stop();
            invokerTask.GetAwaiter().GetResult();

            Console.WriteLine("Stopped.");
            Console.WriteLine("Press enter to terminate.");
            Console.ReadLine();
        }
    }
}
