using System;
using System.Threading;
using System.Threading.Tasks;

namespace Temporal.Prototypes.MockSdk
{
    public interface IWorkflowHandle
    {
        string Namespace { get; }
        string WorkflowId { get; }
        Task DescribeAsync(CancellationToken cancelToken = default);
    }

    public class WorkflowHandle : IWorkflowHandle
    {
        public string Namespace
        {
            get
            {
                Console.WriteLine($"{nameof(WorkflowHandle)}.{nameof(Namespace)} invoked.");
                return null;
            }
        }

        public string WorkflowId
        {
            get
            {
                Console.WriteLine($"{nameof(WorkflowHandle)}.{nameof(WorkflowId)} invoked.");
                return null;
            }
        }

        public Task DescribeAsync(CancellationToken cancelToken = default)
        {
            Console.WriteLine($"{nameof(WorkflowHandle)}.{nameof(DescribeAsync)}(..) invoked.");
            return Task.CompletedTask;
        }
    }
}