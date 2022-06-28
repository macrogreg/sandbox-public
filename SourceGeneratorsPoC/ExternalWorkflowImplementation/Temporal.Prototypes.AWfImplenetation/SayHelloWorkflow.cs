
#pragma warning disable IDE0065  // Misplaced using directive (notmally, usings must be outside the namespace)

namespace Temporal.Samples.VerySimpleWorkflow.WorkflowHost
{

    using System;
    using System.Threading.Tasks;
    using Temporal.Prototypes.MockSdk;
    using Temporal.Samples.VerySimpleWorkflow.ActivityHost;

    public record GreetingKind(string Utterance);
    public record GreetingInfo(int GreetingsCountMax, string PersonName);
    public record CompletedGreetingsInfo(int GreetingsCount);


    [WorkflowImplementation]
    public class SayHelloWorkflow
    {
        private GreetingInfo _greetingSpec = new GreetingInfo(42, "World");
        private int _completedCount = 0;

        [WorkflowMainRoutine]
        public async Task SayManyHellosAsync(GreetingInfo initialGreetingSpec, IWorkflowContext workflowCtx)
        {
            if (initialGreetingSpec != null)
            {
                _greetingSpec = initialGreetingSpec;
            }

            _completedCount = 0;
            while (_completedCount < _greetingSpec.GreetingsCountMax)
            {
                await workflowCtx.Activities.ExecuteAsync("SayHello", new UtteranceInfo(_greetingSpec.PersonName));
                _completedCount++;

                await workflowCtx.SleepAsync(TimeSpan.FromMinutes(1));
            }
        }

        [WorkflowSignalHandler]
        public void UpdateGreetingSpec(GreetingInfo greetingSpec)
        {
            if (greetingSpec != null)
            {
                _greetingSpec = greetingSpec;
            }
        }

        [WorkflowQueryHandler]
        public CompletedGreetingsInfo GetProgressStatus()
        {
            return new CompletedGreetingsInfo(_completedCount);
        }
    }
}

namespace Temporal.Samples.VerySimpleWorkflow.ActivityHost
{
    using System;
    using System.Threading.Tasks;

    public record UtteranceInfo(string PersonName);

    public static class Utterances
    {
        public static async Task SayHelloAsync(UtteranceInfo utteranceSpec)
        {
            string target = utteranceSpec?.PersonName ?? "World";
            Console.WriteLine($"Hello, {target}!");
            await Task.Delay(millisecondsDelay: 1);
        }
    }
}

#pragma warning restore IDE0065  // Misplaced using directive (notmally, usings must be outside the namespace)