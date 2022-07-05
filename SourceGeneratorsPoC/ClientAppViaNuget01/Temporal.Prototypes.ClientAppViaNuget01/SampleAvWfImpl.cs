using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.ClientAppViaNuget01
{
    public record SampleAvWfInput(string Text, int Number);
    public record SampleAvWfResult(IList<string> Lines);


    [WorkflowImplementation(WorkflowTypeName = "SampleAv" + "_Workflow")]
    // [WorkflowImplementation]
    public class SampleAvWfImpl
    {
        [WorkflowMainRoutine]
        public async Task<SampleAvWfResult> ExecWorkflowAsync(SampleAvWfInput input, IWorkflowContext workflowCtx)
        {
            Console.WriteLine($"{nameof(ExecWorkflowAsync)}(..) was invoked.");
            await Task.Delay(millisecondsDelay: 1);

            List<string> lines = new();

            int number = Math.Min(input.Number, 0);
            string text = input.Text ?? String.Empty;

            for (int i = 0; i < number; i++)
            {
                lines.Add($"{i+1}) \"{text}\"");
            }

            return new SampleAvWfResult(lines);
        }

        [WorkflowQueryHandler]
        public int QuerySomeState()
        {
            Console.WriteLine($"{nameof(QuerySomeState)}() was invoked.");
            return 42;
        }

        [WorkflowSignalHandler]
        public async Task HandleASignalAsync(string input)
        {
            Console.WriteLine($"{nameof(HandleASignalAsync)}({nameof(input)}=\"{input}\") was invoked.");
            await Task.Delay(millisecondsDelay: 1);            
        }

        [WorkflowSignalHandler(SignalTypeName = "PatricularSignal")]
        public void HandleAnotherSignal()
        {
            Console.WriteLine($"{nameof(HandleAnotherSignal)}() was invoked.");            
        }

        public async Task AnotherPublicApi(string input)
        {
            Console.WriteLine($"{nameof(AnotherPublicApi)}({nameof(input)}=\"{input}\") was invoked.");
            await Task.Delay(millisecondsDelay: 1);
        }
    }
}
