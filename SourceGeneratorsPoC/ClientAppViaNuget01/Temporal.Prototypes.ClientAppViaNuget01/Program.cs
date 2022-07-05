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

            UseDirectWfFromThisAssembly().GetAwaiter().GetResult();

            UseStubForWfFromThisAssembly().GetAwaiter().GetResult();

            Console.WriteLine($"\"{this.GetType().FullName}\" about to finish. Press enter.");
            Console.ReadLine();

            Console.WriteLine($"\"{this.GetType().FullName}\" stopped.");
        }

        private async Task UseDirectWfFromThisAssembly()
        {
            Console.WriteLine($"\n----------- {nameof(UseDirectWfFromThisAssembly)}() {{");

            SampleAvWfImpl wf = new();

            Task<SampleAvWfResult> wfConclusion = wf.ExecWorkflowAsync(new SampleAvWfInput("Sample-Wf-Input-1", 5), null);
            Console.WriteLine();

            await wf.HandleASignalAsync("Sample-Signal-Input-1");
            Console.WriteLine();

            wf.HandleAnotherSignal();
            Console.WriteLine();

            wf.QuerySomeState();
            Console.WriteLine();

            SampleAvWfResult res = await wfConclusion;
            Console.WriteLine($"res: \"{res?.ToString() ?? "<NULL>"}\".");

            Console.WriteLine("\n----------- }");
        }

        private async Task UseStubForWfFromThisAssembly()
        {
            await Task.Delay(millisecondsDelay: 1);

            Console.WriteLine($"\n----------- {nameof(UseStubForWfFromThisAssembly)}() {{");

            // IWorkflowHandle mockWfHandle = new WorkflowHandle();
            // MockWfStub mockWf = new(mockWfHandle);

            // Task<SampleAvWfResult> mockWfConclusion = mockWf.ExecWorkflowAsync(new SampleAvWfInput("Sample-Wf-Input-2", 10));
            // Console.WriteLine();

            // await mockWf.HandleASignalAsync("Sample-Signal-Input-2");
            // Console.WriteLine();

            // await mockWf.HandleAnotherSignalAsync();
            // Console.WriteLine();

            // await mockWf.QuerySomeStateAsync();
            // Console.WriteLine();

            // SampleAvWfResult res = await mockWfConclusion;
            // Console.WriteLine($"res: \"{res?.ToString() ?? "<NULL>"}\".");

            Console.WriteLine("\n----------- }");
        }
    }
}
