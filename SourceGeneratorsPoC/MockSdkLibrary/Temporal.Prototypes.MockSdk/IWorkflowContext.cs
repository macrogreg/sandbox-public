using System;
using System.Threading;
using System.Threading.Tasks;

namespace Temporal.Prototypes.MockSdk
{
    public interface IWorkflowContext
    {
        string WorkflowId { get; }
        IActivityOrchestrationService Activities { get; }
        Task<bool> SleepAsync(TimeSpan timeSpan, CancellationToken cancelToken = default);
    }

    public interface IActivityOrchestrationService
    {
        Task ExecuteAsync<TArg>(string activityName, TArg input, CancellationToken cancelToken = default);
    }
}