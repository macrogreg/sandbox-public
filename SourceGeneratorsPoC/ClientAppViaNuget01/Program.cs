using System;

namespace Temporal.Prototypes.ClientAppViaNuget01
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

            

            Console.WriteLine($"\"{this.GetType().FullName}\" was stopped.");
        }
    }
}
