#region MAIN_TEMPLATE
using System;
using System.Threading;
using System.Threading.Tasks;
using Temporal.Prototypes.MockSdk;

namespace __Namespace_Template__
{
    /// <summary>
    /// This class is an auto=generated stub for <see cref="__WorkflowImplementationType_Template__" />
    /// (in assembly "__WorkflowImplementationTypeAsm_Template__").
    /// </summary>
    internal partial class __WorkflowStubClass_Template__
    {
        private readonly IWorkflowHandle _workflowHandle;

        public __WorkflowStubClass_Template__(IWorkflowHandle workflowHandle)
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

        /* __SubTemplates_Template__ */
        #region SUB_TEMPLATES

        #region TEMPLATE: ExecWorkflow-1Arg-NotVoid

        public Task<__WfResultType_Template__> __ExecWorkflowMethodName_Template__(__WfInputArgType_Template__ input, CancellationToken cancelToken = default)
        {
            return __ExecWorkflowMethodName_Template__(input, signalConfig: null, cancelToken);
        }

        public async Task<__WfResultType_Template__> __ExecWorkflowMethodName_Template__(__WfInputArgType_Template__ input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string WorkflowTypeName = "__WorkflowTypeName_Template__";

            Console.WriteLine($"__ExecWorkflowMethodName_Template__(..) was invoked to execute the workflow via {this.GetType().Name}:"
                            + $" WorkflowTypeName=\"{WorkflowTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
            return default(__WfResultType_Template__);
        }

        #endregion TEMPLATE: ExecWorkflow-1Arg-NotVoid


        #region TEMPLATE: SendSignal-0Arg

        public Task __SendSignalMethodName_Template__(CancellationToken cancelToken = default)
        {
            return __SendSignalMethodName_Template__(signalConfig: null, cancelToken);
        }

        public async Task __SendSignalMethodName_Template__(SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "__SignalTypeName_Template__";

            Console.WriteLine($"__SendSignalMethodName_Template__(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }

        #endregion TEMPLATE: SendSignal-0Arg


        #region TEMPLATE: SendSignal-1Arg

        public Task __SendSignalMethodName_Template__(__SigInputArgType_Template__ input, CancellationToken cancelToken = default)
        {
            return __SendSignalMethodName_Template__(input, signalConfig: null, cancelToken);
        }

        public async Task __SendSignalMethodName_Template__(__SigInputArgType_Template__ input, SignalWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string SignalTypeName = "__SignalTypeName_Template__";
            
            Console.WriteLine($"__SendSignalMethodName_Template__(..) was invoked to send a Signal via {this.GetType().Name}:"
                            + $" input={typeof(__SigInputArgType_Template__).Name}{{{input}}};"
                            + $" SignalTypeName=\"{SignalTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);
        }

        #endregion TEMPLATE: SendSignal-1Arg


        #region TEMPLATE: ExecQuery-0Arg

        public Task<__QryResultType_Template__> __ExecQueryMethodName_Template__(CancellationToken cancelToken = default)
        {
            return __ExecQueryMethodName_Template__(signalConfig: null, cancelToken);
        }

        public async Task<__QryResultType_Template__> __ExecQueryMethodName_Template__(QueryWorkflowConfiguration signalConfig, CancellationToken cancelToken = default)
        {
            const string QueryTypeName = "__QueryTypeName_Template__";

            Console.WriteLine($"__ExecQueryMethodName_Template__(..) was invoked to send a Query via {this.GetType().Name}:"
                            + $" QueryTypeName=\"{QueryTypeName}\".");

            await Task.Delay(millisecondsDelay: 1);

            return default(__QryResultType_Template__);
        }

        #endregion TEMPLATE: ExecQuery-0Arg

        #endregion SUB_TEMPLATES
    }
}
#endregion MAIN_TEMPLATE


#region TEMPLATE_SUPPORT

namespace Temporal.Prototypes.MockSdk
{
    internal interface IWorkflowHandle
    {
    }

    internal class SignalWorkflowConfiguration
    {
    }

    internal class QueryWorkflowConfiguration
    {
    }
}

namespace __Namespace_Template__
{    
    internal class __WorkflowImplementationType_Template__
    {
    }

    internal class __WfInputArgType_Template__
    {
    }

    internal class __WfResultType_Template__
    {
    }

    internal class __SigInputArgType_Template__
    {
    }

    internal class __QryInputArgType_Template__
    {
    }
    
    internal class __QryResultType_Template__
    {
    }
}

#endregion TEMPLATE_SUPPORT