using System;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;


namespace Temporal.Samples.VerySimpleWorkflow.WorkflowHost
{
    using System;
    using System.Threading.Tasks;
    using Temporal.Samples.VerySimpleWorkflow.ActivityHost;

    public record GreetingInfo(int GreetingsCountMax, string PersonName);
    public record CompletedGreetingsInfo(int GreetingsCount);


    [WorkflowImplementation]
    public class SayHelloWorkflow
    {
        private GreetingInfo _greetingSpec = new GreetingInfo(42, "World");
        private int _completedCount = 0;

        [WorkflowMainRoutine]
        public async Task SayHelloAsync(GreetingInfo initialGreetingSpec, IWorkflowContext workflowCtx)
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

            Delegate y1 = ActivityImpl.Act2;

            var z1 = ActivityImpl.Act2;

            Delegate y2 = ActivityImpl.ActivityMethod;

            var z2 = ActivityImpl.ActivityMethod;

            Action x1 = ActivityImpl.Act1;
            //Action<string> x2 = ActivityImpl.Act1;
            Action<int> x3 = ActivityImpl.Act1;
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

    internal class ActivityImpl
    {
        public void ActivityMethod()
        {
        }

        public static void Act2(double x) { }

        public static void Act1() { }
        public static void Act1(int x) { }
    }
}

namespace Temporal.Samples.VerySimpleWorkflow.ActivityHost
{
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
