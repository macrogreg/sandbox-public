using System;
using System.Linq.Expressions;

namespace Temporal.Prototypes.MockSdk
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    public class WorkflowImplementationAttribute : Attribute
    {
        public string WorkflowTypeName { get; set; }

        public WorkflowImplementationAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class WorkflowMainRoutineAttribute : Attribute
    {
        public WorkflowMainRoutineAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class WorkflowSignalHandlerAttribute : Attribute
    {
        public string SignalTypeName { get; set; }

        public WorkflowSignalHandlerAttribute()            
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class WorkflowQueryHandlerAttribute : Attribute
    {
        public string QueryTypeName { get; set; }

        public WorkflowQueryHandlerAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class WorkflowStubAttribute : Attribute
    {
        public Type WorkflowImplementationType { get; }

        public WorkflowStubAttribute(Type workflowImplementationType)
        {
            WorkflowImplementationType = workflowImplementationType;
        }
    }
}