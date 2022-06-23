using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.AWfImplenetation
{
    public record AWfInput(string Text, int Number);
    public record AWfResult(IList<string> Lines);


    [WorkflowImplementation(WorkflowTypeName = "SomePoCWorkflow")]
    public class AWorkflowImplementation
    {
        [WorkflowMainRoutine]
        public async Task<AWfResult> MainAsync(AWfInput input, IWorkflowContext workflowCtx)
        {
            Console.WriteLine($"{nameof(MainAsync)}(..) was invoked.");
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
        public int RunQuery()
        {
            Console.WriteLine($"{nameof(RunQuery)}() was invoked.");
            return 42;
        }

        [WorkflowSignalHandler]
        public async Task HandleSignal1Async(string input)
        {
            Console.WriteLine($"{nameof(HandleSignal1Async)}({nameof(input)}=\"{input}\") was invoked.");
            await Task.Delay(millisecondsDelay: 1);            
        }

        [WorkflowSignalHandler(SignalTypeName = "PatricularSignal")]
        public void HandleSignal2()
        {
            Console.WriteLine($"{nameof(HandleSignal2)}() was invoked.");            
        }

        public async Task AnotherPublicApi(string input)
        {
            Console.WriteLine($"{nameof(AnotherPublicApi)}({nameof(input)}=\"{input}\") was invoked.");
            await Task.Delay(millisecondsDelay: 1);
        }
    }
}
