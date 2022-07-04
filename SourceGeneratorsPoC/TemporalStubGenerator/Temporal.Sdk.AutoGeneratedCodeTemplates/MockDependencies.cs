using System;
using System.Collections.Generic;
using System.Text;

namespace Temporal.Sdk.StubGenerator.MOckSdkDependencies
{
    // We want to take a dependency on the ADK only by name (not actual type reference).
    // This greatly simplifies compilation/source generation and lso distribution.
    // For convenience, we make sure that the templetes are compilable.
    // So, we need to include mock versions of the SDK types referenced from the templates.

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
