using System;
using System.Threading;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.ClientAppViaNuget01
{
    /// <summary>
    /// This class is an auto=generated stub for <see cref="Temporal.Prototypes.ClientAppViaNuget01.SampleAvWfImpl" />
    /// (in assembly "ClientAppViaNuget01, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").
    /// </summary>
    internal partial class MockWfStub
    {
        private readonly IWorkflowHandle _workflowHandle;

        public MockWfStub(IWorkflowHandle workflowHandle)
        {
            if (workflowHandle == null)
            {
                throw new ArgumentNullException(nameof(workflowHandle));
            }

            _workflowHandle = workflowHandle;
        }

        public IWorkflowHandle WorkflowHandle
        {
            get { return _workflowHandle; }
        }

        
        public Task<Temporal.Prototypes.ClientAppViaNuget01.SampleAvWfResult> ExecWorkflowAsync(Temporal.Prototypes.ClientAppViaNuget01.SampleAvWfInput input, CancellationToken cancelToken = default)
        {
            return ExecWorkflowAsync(input, signalConfig: null, cancelToken);
        }

        public async Task<Temporal.Prototypes.ClientAppViaNuget01.SampleAvWfResult> ExecWorkflowAsync(Temporal.Prototypes.ClientAppViaNuget01.SampleAvWfInput input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string WorkflowTypeName = "ExecWorkflow";

            Console.WriteLine($"ExecWorkflowAsync(..) was invoked [SYNC part] to execute the workflow via {this.GetType().Name}:"
                            + $" WorkflowTypeName=\"{WorkflowTypeName}\".");

            await Task.Delay(millisecondsDelay: 100);

            Console.WriteLine($"ExecWorkflowAsync(..) was invoked [ASYNC part] to execute the workflow via {this.GetType().Name}:"
                            + $" WorkflowTypeName=\"{WorkflowTypeName}\".");

            return default(Temporal.Prototypes.ClientAppViaNuget01.SampleAvWfResult);
        }



        public Task<int> QuerySomeStateAsync(CancellationToken cancelToken = default)
        {
            return QuerySomeStateAsync(signalConfig: null, cancelToken);
        }

        public async Task<int> QuerySomeStateAsync(QueryWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string QueryTypeName = "QuerySomeState";

            Console.WriteLine($"QuerySomeStateAsync(..) was invoked to send a Query via {this.GetType().Name}:"
                            + $" QueryTypeName=\"{QueryTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);

            return default(int);
        }



        public Task HandleASignalAsync(string input, CancellationToken cancelToken = default)
        {
            return HandleASignalAsync(input, signalConfig: null, cancelToken);
        }

        public async Task HandleASignalAsync(string input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "HandleASignal";
            
            Console.WriteLine($"HandleASignalAsync(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" input={typeof(string).Name}{{{input}}};"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }



        public Task HandleAnotherSignalAsync(CancellationToken cancelToken = default)
        {
            return HandleAnotherSignalAsync(signalConfig: null, cancelToken);
        }

        public async Task HandleAnotherSignalAsync(SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "PatricularSignal";

            Console.WriteLine($"HandleAnotherSignalAsync(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }



    }
}
