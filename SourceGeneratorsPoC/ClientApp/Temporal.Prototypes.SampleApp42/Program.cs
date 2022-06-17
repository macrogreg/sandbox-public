using System;
using Temporal.Sdk.Generated;

namespace Temporal.Prototypes.SampleApp42
{
    public class Program
    {
        public static void Main(string[] _)
        {
            (new Program()).Run();
        }

        public void Run()
        {
            Console.WriteLine($"\"{this.GetType().FullName}\" was run.");

            ConsolePrinter printer = new("XYZ");
            printer.WriteLine("Printed using generated code.");


            Console.WriteLine($"\"{this.GetType().FullName}\" was stopped.");
        }
    }
}
