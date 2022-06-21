using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.OtherAssemblyWorkflow
{
    public record AWfInput(string Text, int Number);
    public record AWfResult(IList<string> Lines);


    [WorkflowImplementation(WorkflowTypeName = "BinaryWorkflow")]
    public class OtherAssemblyWorkflowImpl
    {
        [WorkflowMainRoutine]
        public async Task<AWfResult> ExecAsync(AWfInput input, IWorkflowContext workflowCtx)
        {
            Console.WriteLine($"{nameof(ExecAsync)}(..) was invoked.");
            await Task.Delay(millisecondsDelay: 1);

            List<string> lines = new();

            int number = Math.Min(input.Number, 0);
            string text = input.Text ?? String.Empty;

            for (int i = 0; i < number; i++)
            {
                lines.Add($"{i+1}) \"{text}\"");
            }

            return new AWfResult(lines);
        }

        [WorkflowQueryHandler]
        public int RunAQuery()
        {
            Console.WriteLine($"{nameof(RunAQuery)}() was invoked.");
            return 42;
        }

        [WorkflowSignalHandler]
        public async Task HandleSignal01Async(string input)
        {
            Console.WriteLine($"{nameof(HandleSignal01Async)}({nameof(input)}=\"{input}\") was invoked.");
            await Task.Delay(millisecondsDelay: 1);            
        }

        [WorkflowSignalHandler(SignalTypeName = "PatricularSignal")]
        public void HandleSignal02(double input)
        {
            Console.WriteLine($"{nameof(HandleSignal02)}({nameof(input)}={input}) was invoked.");            
        }

        public async Task AnotherPublicApi(string input)
        {
            Console.WriteLine($"{nameof(AnotherPublicApi)}({nameof(input)}=\"{input}\") was invoked.");
            await Task.Delay(millisecondsDelay: 1);
        }
    }
}
