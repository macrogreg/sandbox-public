using System;
using System.Threading;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.OtherAssemblyWorkflow
{
    /// <summary>
    /// This class is an auto=generated stub for <see cref="Temporal.Prototypes.OtherAssemblyWorkflow.OtherAssemblyWorkflowImpl" />
    /// (in assembly "BinaryOnlyWfImplementation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").
    /// </summary>
    internal partial class OtherAssemblyWorkflowStub
    {
        private readonly IWorkflowHandle _workflowHandle;

        public OtherAssemblyWorkflowStub(IWorkflowHandle workflowHandle)
        {
            if (_workflowHandle == null)
            {
                throw new ArgumentNullException(nameof(workflowHandle));
            }

            _workflowHandle = workflowHandle;
        }

        public IWorkflowHandle WorkflowHandle
        {
            get { return _workflowHandle; }
        }

        
        public Task<Temporal.Prototypes.OtherAssemblyWorkflow.AWfResult> ExecAsync(Temporal.Prototypes.OtherAssemblyWorkflow.AWfInput input, CancellationToken cancelToken = default)
        {
            return ExecAsync(input, signalConfig: null, cancelToken);
        }

        public async Task<Temporal.Prototypes.OtherAssemblyWorkflow.AWfResult> ExecAsync(Temporal.Prototypes.OtherAssemblyWorkflow.AWfInput input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string WorkflowTypeName = "Exec";

            Console.WriteLine($"ExecAsync(..) was invoked to execute the workflow via {this.GetType().Name}:"
                            + $" WorkflowTypeName=\"{WorkflowTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
            return default(Temporal.Prototypes.OtherAssemblyWorkflow.AWfResult);
        }



        public Task<int> RunAQueryAsync(CancellationToken cancelToken = default)
        {
            return RunAQueryAsync(signalConfig: null, cancelToken);
        }

        public async Task<int> RunAQueryAsync(QueryWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string QueryTypeName = "RunAQuery";

            Console.WriteLine($"RunAQueryAsync(..) was invoked to send a Query via {this.GetType().Name}:"
                            + $" QueryTypeName=\"{QueryTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);

            return default(int);
        }



        public Task HandleSignal01Async(string input, CancellationToken cancelToken = default)
        {
            return HandleSignal01Async(input, signalConfig: null, cancelToken);
        }

        public async Task HandleSignal01Async(string input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "HandleSignal01";
            
            Console.WriteLine($"HandleSignal01Async(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" input={typeof(string).Name}{{{input}}};"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }



        public Task HandleSignal02Async(CancellationToken cancelToken = default)
        {
            return HandleSignal02Async(signalConfig: null, cancelToken);
        }

        public async Task HandleSignal02Async(SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "PatricularSignal";

            Console.WriteLine($"HandleSignal02Async(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }



    }
}
