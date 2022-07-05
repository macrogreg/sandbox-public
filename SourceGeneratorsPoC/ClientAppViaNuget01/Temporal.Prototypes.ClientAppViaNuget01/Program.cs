using System;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;

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

            UseWfFromThisAssembly().GetAwaiter().GetResult();


            Console.WriteLine($"\"{this.GetType().FullName}\" about to finish. Press enter.");
            Console.ReadLine();

            Console.WriteLine($"\"{this.GetType().FullName}\" stopped.");
        }

        private async Task UseWfFromThisAssembly()
        {
            IWorkflowHandle mockWfHandle = new WorkflowHandle();
            MockWfStub mockWf = new(mockWfHandle);

            Task<SampleAvWfResult> mockWfConclusion = mockWf.ExecWorkflowAsync(new SampleAvWfInput("Sample-Wf-Input-1", 42));
            Console.WriteLine();

            await mockWf.HandleASignalAsync("Sample-Signal-Input-1");
            Console.WriteLine();

            await mockWf.HandleAnotherSignalAsync();
            Console.WriteLine();

            await mockWf.QuerySomeStateAsync();
            Console.WriteLine();

            SampleAvWfResult res = await mockWfConclusion;
            Console.WriteLine($"res: \"{res?.ToString() ?? "<NULL>"}\".");

            Console.WriteLine();
        }
    }
}
