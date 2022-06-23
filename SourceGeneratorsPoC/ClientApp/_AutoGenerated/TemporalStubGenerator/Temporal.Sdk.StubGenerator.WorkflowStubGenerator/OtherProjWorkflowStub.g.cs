using System;
using System.Threading;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.SampleApp42
{
    /// <summary>
    /// This class is an auto=generated stub for <see cref="Temporal.Prototypes.AWfImplenetation.AWorkflowImplementation" />
    /// (in assembly "ExternalWorkflowImplementation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null").
    /// </summary>
    internal partial class OtherProjWorkflowStub
    {
        private readonly IWorkflowHandle _workflowHandle;

        public OtherProjWorkflowStub(IWorkflowHandle workflowHandle)
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

        
        public Task<Temporal.Prototypes.AWfImplenetation.AWfResult> MainAsync(Temporal.Prototypes.AWfImplenetation.AWfInput input, CancellationToken cancelToken = default)
        {
            return MainAsync(input, signalConfig: null, cancelToken);
        }

        public async Task<Temporal.Prototypes.AWfImplenetation.AWfResult> MainAsync(Temporal.Prototypes.AWfImplenetation.AWfInput input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string WorkflowTypeName = "Main";

            Console.WriteLine($"MainAsync(..) was invoked to execute the workflow via {this.GetType().Name}:"
                            + $" WorkflowTypeName=\"{WorkflowTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
            return default(Temporal.Prototypes.AWfImplenetation.AWfResult);
        }



        public Task<int> RunQueryAsync(CancellationToken cancelToken = default)
        {
            return RunQueryAsync(signalConfig: null, cancelToken);
        }

        public async Task<int> RunQueryAsync(QueryWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string QueryTypeName = "RunQuery";

            Console.WriteLine($"RunQueryAsync(..) was invoked to send a Query via {this.GetType().Name}:"
                            + $" QueryTypeName=\"{QueryTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);

            return default(int);
        }



        public Task HandleSignal1Async(string input, CancellationToken cancelToken = default)
        {
            return HandleSignal1Async(input, signalConfig: null, cancelToken);
        }

        public async Task HandleSignal1Async(string input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "HandleSignal1";
            
            Console.WriteLine($"HandleSignal1Async(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" input={typeof(string).Name}{{{input}}};"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }



        public Task HandleSignal2Async(CancellationToken cancelToken = default)
        {
            return HandleSignal2Async(signalConfig: null, cancelToken);
        }

        public async Task HandleSignal2Async(SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "PatricularSignal";

            Console.WriteLine($"HandleSignal2Async(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }



    }
}
