using Temporal.Prototypes.MockSdk;

namespace Temporal.Prototypes.SampleApp42
{
    [WorkflowStub(typeof(SampleAvWorkflowImplementation))]
    internal partial class MockWorkflowStub : IWorkflowStub
    {        
    }
}
