using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Temporal.Sdk.StubGenerator
{
    [Generator]
    public class WorkflowStubGenerator : IIncrementalGenerator
    {
        private static class WfStubAttr
        {
            public const string TypeName = "WorkflowStubAttribute";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;
            public const string PrNmWfImplType = "WorkflowImplementationType";
        }

        private static class WfStubIface
        {
            public const string TypeName = "IWorkflowStub";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;
        }

        private static class WfImplAttr
        {
            public const string TypeName = "WorkflowImplementationAttribute";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;
            public const string PrNmWfTypeName = "WorkflowTypeName";
        }

        private static class WfMainRoutineImplAttr
        {
            public const string TypeName = "WorkflowMainRoutineAttribute";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;            
        }

        private static class WfSigHndlImplAttr
        {
            public const string TypeName = "WorkflowSignalHandlerAttribute";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;
            public const string PrNmSigTypeName = "SignalTypeName";
        }

        private static class WfQryHndlImplAttr
        {
            public const string TypeName = "WorkflowQueryHandlerAttribute";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;
            public const string PrNmQryTypeName = "QueryTypeName";
        }

        private static class WfCtxIface
        {
            public const string TypeName = "IWorkflowContext";
            public const string Namespace = "Temporal.Prototypes.MockSdk";
            public const string FullName = Namespace + "." + TypeName;
        }

        private static class Diagnostics
        {
            public const string Category = "Temporal.StubGenerators";
            public const string SdkIssuesPage = "https://github.com/temporalio/sdk-dotnet/issues";

            /// <summary>
            /// Message Parameters: {0} = {message}.
            /// </summary>
            public static readonly DiagnosticDescriptor TMPRL001 = new(
                    id: "TMPRL001",
                    title: $"Internal critical error in `{typeof(WorkflowStubGenerator).FullName}`",
                    messageFormat: "{0}",
                    category: Category,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

            /// <summary>
            /// Message Parameters: {0} = {message}.
            /// </summary>
            public static readonly DiagnosticDescriptor TMPRL002 = new(
                    id: "TMPRL002",
                    title: $"Internal error in `{typeof(WorkflowStubGenerator).FullName}`",
                    messageFormat: "{0}",
                    category: Category,
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

            /// <summary>
            /// @ToDo: We must remove all occurances of this on a prod release.
            /// However, it is very convenient to have during prototyping de debugging of the generator.
            /// Message Parameters: {0} = {message}.
            /// </summary>
            public static readonly DiagnosticDescriptor TMPRL003 = new(
                    id: "TMPRL003",
                    title: $"An unspecified syntax error occurred during source generation",
                    messageFormat: "{0}",
                    category: Category,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

            /// <summary>
            /// Message Parameters: {1} = {wfImplTypeName}; {1} = {wfImplTypeNamespace}; {2} = {wfStubTypeFullName}.
            /// </summary>
            public static readonly DiagnosticDescriptor TMPRL010 = new(
                    id: "TMPRL010",
                    title: $"Workflow implementation does not explicitly specify the Temporal {WfImplAttr.PrNmWfTypeName}",
                    messageFormat: $"The type `{{0}}` in namespace `{{1}}` is decorated with the `{WfImplAttr.TypeName}`,"
                                 + $" but `{WfImplAttr.PrNmWfTypeName}` is not specified. A fallback {WfImplAttr.PrNmWfTypeName}"
                                 + $" based on the name of the .NET type (\"{{0}}\") will be used while generating the Workflow Stub `{{2}}`.",
                    category: Category,
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

            /// <summary>
            /// Message Parameters: {0} = {wfStubTypeFullName}.
            /// </summary>
            public static readonly DiagnosticDescriptor TMPRL011 = new(
                    id: "TMPRL011",
                    title: $"Type implements `{WfStubIface.TypeName}` but is not decorated with `{WfStubAttr.TypeName}`",
                    messageFormat: $"The type `{{0}}` implements the interface `{WfStubIface.FullName}`, but it is not decorated with"
                                 + $" `{WfStubAttr.FullName}`. A Workflow Stub will NOT be generated."
                                 + $" To generate a Workflow Stub, a partial class must both: implement `{WfStubIface.TypeName}`"
                                 + $" and be decorated with `{WfStubAttr.TypeName}`.",
                    category: Category,
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true);

            /// <summary>
            /// Message Parameters: {0} = {wfStubTypeFullName}.
            /// </summary>
            public static readonly DiagnosticDescriptor TMPRL012 = new(
                    id: "TMPRL012",
                    title: $"Type is decorated with `{WfStubAttr.TypeName}`, but does not implement `{WfStubIface.TypeName}`",
                    messageFormat: $"The type `{{0}}` is decorated with `{WfStubAttr.FullName}`, but it does not directly implement"
                                 + $" the interface `{WfStubIface.FullName}`."
                                 + $" A type decorated with `{WfStubAttr.TypeName}` is required to DIRECTLY (i.e. non-transitively)"
                                 + $" implement `{WfStubIface.TypeName}`. A Workflow Stub will NOT be generated.",
                    category: Category,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);

            public static Diagnostic Create(DiagnosticDescriptor descriptor, ImmutableArray<Location> locations, params object[] messageArgs)
            {
                Location mainLocation = Location.None;
                IEnumerable<Location> otherLocations = null;

                if (locations != null)
                {
                    if (locations.Length == 1)
                    {
                        mainLocation = locations[0];
                    }
                    else if (locations.Length > 1)
                    {
                        mainLocation = locations[0];
                        otherLocations = locations.Skip(1);
                    }
                }

                return Create(descriptor, mainLocation, otherLocations, messageArgs);
            }

            public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs)
            {
                return Create(descriptor, location, otherLocations: null, messageArgs);
            }

            public static Diagnostic Create(DiagnosticDescriptor descriptor,
                                            Location mainLocation,
                                            IEnumerable<Location> otherLocations,
                                            params object[] messageArgs)
            {
                return Diagnostic.Create(descriptor, mainLocation, otherLocations, messageArgs);
            }

        }

        /// <summary>
        /// Entry point into the incremental source generator.
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Filter down to classes marked with `WorkflowStubAttribute` or `IWorkflowStub`:
            IncrementalValuesProvider<ClassDeclarationSyntax> markedClassDeclarations
                    = context.SyntaxProvider.CreateSyntaxProvider(
                                    predicate: IsSyntaxClassDeclarationWithAttributes,
                                    transform: FilterForMarkedWorkflowStubDeclarations)
                            .Where(static (cd) => cd != null);

            // Merge filter results with compilation:
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndMarkedClasses
                    = context.CompilationProvider.Combine(markedClassDeclarations.Collect());

            // Register the main source generation routine:
            context.RegisterSourceOutput(compilationAndMarkedClasses, ExecuteSourceGeneration);
        }

        /// <summary>
        /// Super fast 1st-stage filter:
        /// Checks if a node is a class AND has at least one attribute.
        /// </summary>
        private static bool IsSyntaxClassDeclarationWithAttributes(SyntaxNode node, CancellationToken _)
        {
            if (node is not ClassDeclarationSyntax classDeclarationNode)
            {
                return false;
            }

            return (classDeclarationNode.AttributeLists.Count > 0)
                    || (classDeclarationNode.BaseList != null
                            && classDeclarationNode.BaseList.Types != null
                            && classDeclarationNode.BaseList.Types.Count > 0);
        }

        /// <summary>
        /// Super fast 2nd-stage filter:
        /// If a node is marked with `WorkflowStubAttribute` or implenents `IWorkflowStub`, return the node.
        /// Otherwise return null.
        /// </summary>        
        private static ClassDeclarationSyntax FilterForMarkedWorkflowStubDeclarations(GeneratorSyntaxContext context,
                                                                                      CancellationToken cancelToken)
        {
            ClassDeclarationSyntax classDeclNode = (ClassDeclarationSyntax) context.Node;
            SemanticModel semanticModel = context.SemanticModel;

            if (ImplementsIWorkflowStub(classDeclNode, semanticModel, cancelToken) 
                    || HasWorkflowImplementationAttribute(classDeclNode, semanticModel, cancelToken))
            {
                return (ClassDeclarationSyntax) context.Node;
            }

            return null;
        }

        /// <summary>
        /// Check if classDeclNode is marked with `WorkflowStubAttribute`.
        /// </summary>
        private static bool HasWorkflowImplementationAttribute(ClassDeclarationSyntax classDeclNode,
                                                               SemanticModel semanticModel,
                                                               CancellationToken cancelToken)
        {
            return HasWorkflowImplementationAttribute(classDeclNode, semanticModel, out _, cancelToken);
        }

        /// <summary>
        /// Check if classDeclNode is marked with `WorkflowStubAttribute` and get that attribute.
        /// </summary>        
        private static bool HasWorkflowImplementationAttribute(ClassDeclarationSyntax classDeclNode,
                                                               SemanticModel semanticModel,
                                                               out AttributeSyntax workflowStubAttributeNode,
                                                               CancellationToken cancelToken)
        {
            // Loop through the attributes on the class (group -> individual Attrs):
            foreach (AttributeListSyntax attributeListNode in classDeclNode.AttributeLists)
            {
                foreach (AttributeSyntax attributeNode in attributeListNode.Attributes)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        workflowStubAttributeNode = null;
                        return false;
                    }

                    // Fast check: the name of the Attribute Node must end in `WorkflowStubAttribute`:
                    string attributeNodeName = attributeNode?.Name?.ToString();
                    if (attributeNodeName == null && !attributeNodeName.Trim().EndsWith(WfStubAttr.TypeName))
                    {
                        continue;
                    }

                    // If fast check passed, now chcek the full Attribute type name including namespace by using the semantic model:

                    // The Attribute is represented by it's Ctor (a kind of method).
                    // Get the symbol, navigate to the containing type and chekc the full type name:

                    ISymbol attributeNodeSymbol = semanticModel.GetSymbolInfo(attributeNode).Symbol;
                    if (attributeNodeSymbol is not IMethodSymbol attributeMethodSymbol)
                    {
                        throw new Exception($"{nameof(attributeNodeSymbol)} was expected to be a"
                                          + $" `{nameof(IMethodSymbol)}`, but is was actually a `{attributeNodeSymbol.GetType()}`.");
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeMethodSymbol.ContainingType;
                    string attributeTypeFullName = attributeContainingTypeSymbol.ToDisplayString();

                    // Is the attribute the WorkflowStubAttribute attribute?
                    if ("Temporal.Prototypes.MockSdk.WorkflowStubAttribute".Equals(attributeTypeFullName, StringComparison.Ordinal))
                    {
                        workflowStubAttributeNode = attributeNode;
                        return true;
                    }
                }
            }

            workflowStubAttributeNode = null;
            return false;
        }

        /// <summary>
        /// Check whether classDeclNode implements the `IWorkflowStub`-iface.
        /// </summary>
        private static bool ImplementsIWorkflowStub(ClassDeclarationSyntax classDeclNode,
                                                    SemanticModel semanticModel,
                                                    CancellationToken cancelToken)
        {
            // Attention: This will MISS any non-explicit / non-direct implementations!!
            // We could remove the initial fast check and detect all ifaces,
            // but we prefer to require it and to issue a diagnostic error later if we detect the attribute, but not the iface.

            bool isMatchingBaseNodeNameFound = false;

            BaseListSyntax baseListNode = classDeclNode.BaseList;
            if (baseListNode != null)
            {
                foreach (BaseTypeSyntax baseTypeNode in classDeclNode.BaseList.Types)
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    // Fast check using the syntax node (string compare):
                    string baseTypeNodeStr = baseTypeNode?.ToString();
                    if (baseTypeNodeStr != null && baseTypeNodeStr.EndsWith(WfStubIface.TypeName, StringComparison.Ordinal))
                    {
                        isMatchingBaseNodeNameFound = true;
                        break;
                    }
                }
            }

            if (!isMatchingBaseNodeNameFound)
            {
                return false;
            }

            // Fast check succeeded. Now use semantic model to make sure there is a match:

            ISymbol wfStubTypeDeclarationSymbol = semanticModel.GetDeclaredSymbol(classDeclNode);
            if (wfStubTypeDeclarationSymbol is not ITypeSymbol wfStubTypeSymbol)
            {
                // SourceProductionContext.ReportDiagnostic(..) not in scope. How can we report internal error for this item
                // without affecting other items?
                return false;  
            }

            ImmutableArray<INamedTypeSymbol> ifaces = wfStubTypeSymbol.AllInterfaces;
            foreach(INamedTypeSymbol iface in ifaces)
            {
                string ifaceTypeName = iface.Name;
                string ifaceNamespace = iface.ContainingNamespace?.ToString();
                //string ifaceAssembly = iface.ContainingAssembly?.Identity.ToString();

                if (WfStubIface.TypeName.Equals(ifaceTypeName, StringComparison.Ordinal)
                        && WfStubIface.Namespace.Equals(ifaceNamespace, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Main entry point into the source generation.
        /// </summary>
        private static void ExecuteSourceGeneration(SourceProductionContext srcProdCtx,
                                                    (Compilation, ImmutableArray<ClassDeclarationSyntax>) compileInput)
        {
            Compilation compilation = compileInput.Item1;
            ImmutableArray<ClassDeclarationSyntax> markedClassDecls = compileInput.Item2;

            foreach (ClassDeclarationSyntax classDeclNode in markedClassDecls)
            {
                GenerateWorkflowStub(srcProdCtx, compilation, classDeclNode);
            }
        }

        private static void GenerateWorkflowStub(SourceProductionContext srcProdCtx,
                                                 Compilation compilation,
                                                 ClassDeclarationSyntax wfStubClassDeclNode)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(wfStubClassDeclNode.SyntaxTree);

            bool hasWfImplAttr = HasWorkflowImplementationAttribute(wfStubClassDeclNode,
                                                                    semanticModel,
                                                                    out AttributeSyntax workflowStubAttributeNode,
                                                                    srcProdCtx.CancellationToken);

            bool implementsIWfStub = ImplementsIWorkflowStub(wfStubClassDeclNode, semanticModel, srcProdCtx.CancellationToken);

            // Fail if we marked the input node incorrectly:

            if (!hasWfImplAttr && !implementsIWfStub)
            {
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL001,
                        wfStubClassDeclNode.GetLocation(),
                        $"`{nameof(GenerateWorkflowStub)}` was invoked for a `{nameof(wfStubClassDeclNode)}` for which"
                      + $" {nameof(hasWfImplAttr)}={nameof(implementsIWfStub)}=False. There must be a bug in"
                      + $" {nameof(WorkflowStubGenerator)}. Please report ({Diagnostics.SdkIssuesPage})."));
                return;
            }

            // Parse the semantics of wfStubClassDeclNode's type declaration:

            ISymbol wfStubTypeDeclarationSymbol = semanticModel.GetDeclaredSymbol(wfStubClassDeclNode);
            if (wfStubTypeDeclarationSymbol == null
                    || wfStubTypeDeclarationSymbol is not ITypeSymbol wfStubTypeSymbol)
            {
                string wfStubClassDeclNodeIdentifier = wfStubClassDeclNode?.Identifier.ToString() ?? nameof(wfStubClassDeclNodeIdentifier);
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL001,
                        wfStubClassDeclNode.GetLocation(),
                        $"The syntactic class declaration for `{wfStubClassDeclNodeIdentifier}` cannot be cannot be sematically"
                      + $" interpreted as a type definition. There must be a bug in {nameof(WorkflowStubGenerator)}."
                      + $" Please report ({Diagnostics.SdkIssuesPage})."));
                return;
            }

            string wfStubTypeFullName = wfStubTypeSymbol.ToString();
            string wfStubTypeName = wfStubTypeSymbol.Name;
            string wfStubTypeNamespace = wfStubTypeSymbol.ContainingNamespace?.ToString() ?? String.Empty;

            if (String.IsNullOrWhiteSpace(wfStubTypeName))
            {
                // @ToDo: replace with specific error type.
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL003,
                        wfStubTypeSymbol.Locations,
                        $"`wfStubTypeSymbol.Name` is null or white-space-only."));
            }

            // Ensure that we have both, the marker attribute and the marker iface.
            // If we do not, give up with a descriptive error:

            if (!hasWfImplAttr)
            {
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL011,
                        wfStubClassDeclNode.GetLocation(),
                        wfStubTypeFullName));
                return;
            }

            if (!implementsIWfStub)
            {
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL012,
                        wfStubClassDeclNode.GetLocation(),
                        wfStubTypeFullName));
                return;
            }

            // Ok, the marker attribute and the marker iface are present.
            // Find the workflow implementation to be stubbed by examining the parameter to the `WorkflowStubAttribute` ctor:

            if (workflowStubAttributeNode.ArgumentList?.Arguments == null
                    || workflowStubAttributeNode.ArgumentList.Arguments.Count < 1
                    || workflowStubAttributeNode.ArgumentList.Arguments[0] == null)
            {
                // @ToDo: replace with specific error type.
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL003,
                        wfStubClassDeclNode.GetLocation(),
                        $"Cannot access the first ctor argument for the `WorkflowStubAttribute` that decorates `wfStubTypeFullName`."
                      + $" (include Arguments.Count is accessible)"));
                
                return;
            }

            ExpressionSyntax wfImplTypeArgExpressionNode = workflowStubAttributeNode.ArgumentList.Arguments[0].Expression;
            
            ITypeSymbol wfImplTypeSymbol;

            if (wfImplTypeArgExpressionNode is TypeOfExpressionSyntax typeofExpressionNode)
            {
                wfImplTypeSymbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(semanticModel, typeofExpressionNode.Type).Type;
            }
            else
            {
                wfImplTypeSymbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(semanticModel, wfImplTypeArgExpressionNode).Type;
            }

            if (wfImplTypeSymbol == null)
            {
                // @ToDo: replace with specific error type.
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL003,
                        wfStubClassDeclNode.GetLocation(),
                        $"Not able to extract the `{WfStubAttr.PrNmWfImplType}` from the ctor parameter of the `{WfStubAttr.TypeName}`"
                      + $" that decorates the type `{wfStubTypeFullName}`. The ctor parameter must describe the type that defines"
                      + $" the workflow implementation. Instead, the first ctor param is: \"{wfImplTypeArgExpressionNode}\"."));

                return;
            }

            // Now the `wfImplTypeSymbol` refers to the type the should implement the workflow.

            string wfImplTypeName = wfImplTypeSymbol.Name;
            string wfImplTypeNamespace = wfImplTypeSymbol.ContainingNamespace?.ToString();
            string wfImplFullName = wfImplTypeNamespace + "." + wfImplTypeName;
            string wfImplTypeAssembly = wfImplTypeSymbol.ContainingAssembly?.Identity.ToString();

            // Look for the `WorkflowImplementationAttribute`:

            AttributeData wfImplAttribute = null;
            ImmutableArray<AttributeData> wfImplTypeAttributes = wfImplTypeSymbol.GetAttributes();
            foreach (AttributeData wfImpTAtt in wfImplTypeAttributes)
            {
                string wfImpTAttClassName = wfImpTAtt.AttributeClass?.ToString();
                if (WfImplAttr.FullName.Equals(wfImpTAttClassName))
                {
                    wfImplAttribute = wfImpTAtt;
                    break;
                }
            }

            if (wfImplAttribute == null)
            {
                // @ToDo: replace with specific error type.
                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL003,
                        wfStubClassDeclNode.GetLocation(),
                        $"Type `{wfImplTypeName}` in namespace `{wfImplTypeNamespace}` is expected to define a workflow"
                      + $" implementation becasue it was referenced as `{WfStubAttr.PrNmWfImplType}` by the `{WfStubAttr.TypeName}`"
                      + $" which decorates the type `{wfStubTypeFullName}`. However, `{wfImplTypeName}` is not decorated with"
                      + $" `{WfImplAttr.FullName}`. A type that defines a workflow implementation must be decorated with a"
                      + $" `{WfImplAttr.TypeName}`."));

                return;
            }

            // The workflow implementation has the `WorkflowImplementationAttribute`.
            // It *optionally* has the Temporal WorkflowTypeName.
            // Get the WorkflowTypeName either from the attribute or fall back to the type name.

            string temporalWorkflowTypeName = null;

            ImmutableArray<KeyValuePair<string, TypedConstant>> attrArgs = wfImplAttribute.NamedArguments;
            foreach (KeyValuePair<string, TypedConstant> attrArg in attrArgs)
            {
                if (WfImplAttr.PrNmWfTypeName.Equals(attrArg.Key, StringComparison.Ordinal))
                {
                    temporalWorkflowTypeName = attrArg.Value.Value?.ToString();
                    break;
                }
            }

            if (temporalWorkflowTypeName == null)
            {
                // Did not find wf type name in attribute. Use implementation class name.

                srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                        Diagnostics.TMPRL010,
                        wfImplTypeSymbol.Locations,
                        wfImplTypeName, wfImplTypeNamespace, wfStubTypeFullName));


                temporalWorkflowTypeName = wfImplTypeName;
            }

            // Scan the `wfImplTypeSymbol`, detecting all workflow APIs using the appropriate attributes
            // and issue the corresponding code for the stub methods.

            StringBuilder generatedStubMainSection = WorkflowStubTemplateProvider.SingletonInstance.GetMainSection(
                    wfStubTypeNamespace,
                    wfStubTypeName,
                    wfImplFullName,
                    wfImplTypeAssembly);

            List<StringBuilder> generatedStubSubsections = new();

            ImmutableArray<ISymbol> wfImplMembers = wfImplTypeSymbol.GetMembers();
            foreach (ISymbol wfImplMember in wfImplMembers)
            {
                if (wfImplMember is not IMethodSymbol wfImplMethod)
                {
                    continue;
                }

                ImmutableArray<AttributeData> wfImplMethodAttributes = wfImplMethod.GetAttributes();
                foreach (AttributeData wfImpMethAtt in wfImplMethodAttributes)
                {
                    try
                    {
                        const string AsyncSuffix = "Async";
                        string wfImpMethAttClassName = wfImpMethAtt.AttributeClass?.ToString();

                        // If the attribute `wfImpMethAtt` matched one of the Implemenation-marker-attributes:
                        if (WfMainRoutineImplAttr.FullName.Equals(wfImpMethAttClassName)
                                || WfSigHndlImplAttr.FullName.Equals(wfImpMethAttClassName)
                                || WfQryHndlImplAttr.FullName.Equals(wfImpMethAttClassName))
                        {
                            // Construct stub method name:
                            string methodName = wfImplMethod.Name.Trim();
                            if (!methodName.EndsWith(AsyncSuffix, StringComparison.OrdinalIgnoreCase))
                            {
                                methodName += AsyncSuffix;
                            }

                            // Get the Wf-Type-Name or Signal-Type-Name or Query-Type-Name (respectively) from the attribute:
                            string attrPropNameForType = wfImpMethAttClassName switch
                            {
                                WfMainRoutineImplAttr.FullName => null,
                                WfSigHndlImplAttr.FullName => WfSigHndlImplAttr.PrNmSigTypeName,
                                WfQryHndlImplAttr.FullName => WfQryHndlImplAttr.PrNmQryTypeName,
                                _ => null,
                            };

                            string temporalTypeName = wfImpMethAtt.NamedArguments.FirstOrDefault(na => na.Key.Equals(attrPropNameForType))
                                                                                 .Value.Value?.ToString();

                            if (String.IsNullOrWhiteSpace(temporalTypeName))
                            {
                                temporalTypeName = methodName;

                                if (temporalTypeName.EndsWith(AsyncSuffix, StringComparison.OrdinalIgnoreCase))
                                {
                                    temporalTypeName = temporalTypeName.Substring(0, temporalTypeName.Length - AsyncSuffix.Length);
                                }
                            }

                            bool lastArgIsContext = false;
                            if (wfImplMethod.Parameters.Length > 0)
                            {
                                lastArgIsContext = WfCtxIface.FullName.Equals(wfImplMethod.Parameters[wfImplMethod.Parameters.Length - 1].Type.ToString());
                            }

                            StringBuilder subsection = wfImpMethAttClassName switch
                            {
                                WfMainRoutineImplAttr.FullName => GenerateWorkflowMainRoutineStub(srcProdCtx,
                                                                                                  temporalTypeName,
                                                                                                  wfImplMethod,
                                                                                                  methodName,
                                                                                                  lastArgIsContext),
                                WfSigHndlImplAttr.FullName => GenerateWorkflowSignalHandlerStub(srcProdCtx,
                                                                                                temporalTypeName,
                                                                                                wfImplMethod,
                                                                                                methodName,
                                                                                                lastArgIsContext),
                                WfQryHndlImplAttr.FullName => GenerateWorkflowQueryHandlerStub(srcProdCtx,
                                                                                               temporalTypeName,
                                                                                               wfImplMethod,
                                                                                               methodName,
                                                                                               lastArgIsContext),
                                _ => null,
                            }; 

                            if (subsection != null)
                            {
                                generatedStubSubsections.Add(subsection);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // @ToDo: replace with specific error type.
                        srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                                Diagnostics.TMPRL003,
                                wfImplMethod.Locations,
                                $"Exception in source generator:"
                              + $" {ex}"));
                    }
                }  // foreach (AttributeData wfImpMethAtt in wfImplMethodAttributes)
            }

            string generatedStubSrc = WorkflowStubTemplateProvider.SingletonInstance.MergeSections(generatedStubMainSection,
                                                                                                   generatedStubSubsections);
            srcProdCtx.AddSource(wfStubTypeName + ".g.cs", SourceText.From(generatedStubSrc, Encoding.UTF8));
        }

        private static string GetMethodSignature(IMethodSymbol method)
        {
            if (method == null)
            {
                return "null";
            }

            string typeName = method.ContainingType?.ToString();
            if (!String.IsNullOrEmpty(typeName))
            {
                typeName += ".";
            }

            string signature = method.ToDisplayString();

            if (signature.StartsWith(typeName))
            {
                signature = signature.Substring(typeName.Length);
            }

            return signature;
        }


        // Below APIs are not yet implemented. Just issuing errors as a means of printing progress so far:

        private static StringBuilder GenerateWorkflowMainRoutineStub(SourceProductionContext srcProdCtx,
                                                                     string workflowTypeName,
                                                                     IMethodSymbol stubbedImplMethodSymbol,
                                                                     string stubbedImplMethodName,
                                                                     bool lastArgIsContext)
        {
            string stubReturnType;
            string implReturnType = stubbedImplMethodSymbol.ReturnType?.ToString() ?? "void";

            // @ToDo: Enforce that a query implementation always returns Task<T> or some T.

            if (implReturnType.Equals("void"))
            {
                throw new NotImplementedException("Support for void ExecWorkflow(..) methods is not yet implemented.");
            }
            if (implReturnType.Equals("System.Threading.Tasks.Task"))
            {
                throw new NotImplementedException("Support for Task ExecWorkflowAsync(..) methods is not yet implemented.");
            }
            else if (implReturnType.StartsWith("System.Threading.Tasks.Task<") && implReturnType.EndsWith(">"))
            {
                stubReturnType = implReturnType.Substring("System.Threading.Tasks.Task<".Length, implReturnType.Length - "System.Threading.Tasks.Task<".Length - 1);
            }
            else
            {
                stubReturnType = implReturnType;
            }

            ImmutableArray<IParameterSymbol> args = stubbedImplMethodSymbol.Parameters;
            if ((args.Length == 0)
                    || (args.Length == 1 && lastArgIsContext))
            {
                throw new NotImplementedException("Support for Main Workflow Routine Stubs with ZERO data input arguments is not yet implemented.");
            }
            
            if ((args.Length == 1 && !lastArgIsContext)
                    || (args.Length == 2 && lastArgIsContext))
            {
                return WorkflowStubTemplateProvider.SingletonInstance.GetExecWorkflow1ArgNotVoidSection(stubbedImplMethodName,
                                                                                                        args[0].Type.ToString(),
                                                                                                        stubReturnType,
                                                                                                        workflowTypeName);
            }

            // @ToDo: replace with specific error type.
            srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                    Diagnostics.TMPRL003,
                    stubbedImplMethodSymbol.Locations,
                    $"Invalid Main Workflow Execution Routine signature."
                  + $" A Main Workflow Execution Routine method may have no more than 2 total arguments,"
                  + $" including: 0 or 1 data input arguments, followed by an optional `{WfCtxIface.TypeName}`-argument."
                  + $" However, the method `{stubbedImplMethodName}` is marked with a [{WfMainRoutineImplAttr.TypeName}]"
                  + $" and has the following invalid signature: `{GetMethodSignature(stubbedImplMethodSymbol)}`."
                  + $" A stub for this method will NOT be generated."));

            return null;
        }

        private static StringBuilder GenerateWorkflowSignalHandlerStub(SourceProductionContext srcProdCtx,
                                                                       string signalTypeName,
                                                                       IMethodSymbol stubbedImplMethodSymbol,
                                                                       string stubbedImplMethodName,
                                                                       bool lastArgIsContext)
        {
            ImmutableArray<IParameterSymbol> args = stubbedImplMethodSymbol.Parameters;
            if ((args.Length == 0)
                    || (args.Length == 1 && lastArgIsContext))
            {
                return WorkflowStubTemplateProvider.SingletonInstance.GetSendSignal0ArgSection(stubbedImplMethodName, signalTypeName);
            }

            if ((args.Length == 1 && !lastArgIsContext)
                    || (args.Length == 2 && lastArgIsContext))
            {
                return WorkflowStubTemplateProvider.SingletonInstance.GetSendSignal1ArgSection(stubbedImplMethodName,
                                                                                               args[0].Type.ToString(),
                                                                                               signalTypeName);
            }

            // @ToDo: replace with specific error type.
            srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                    Diagnostics.TMPRL003,
                    stubbedImplMethodSymbol.Locations,
                    $"Invalid Signal Handler signature."
                  + $" A Signal Handler method may have no more than 2 total arguments,"
                  + $" including: 0 or 1 data input arguments, followed by an optional `{WfCtxIface.TypeName}`-argument."
                  + $" However, the method `{stubbedImplMethodName}` is marked with a [{WfSigHndlImplAttr.TypeName}]"
                  + $" and has the following invalid signature: `{GetMethodSignature(stubbedImplMethodSymbol)}`."
                  + $" A stub for this method will NOT be generated."));

            return null;
        }

        private static StringBuilder GenerateWorkflowQueryHandlerStub(SourceProductionContext srcProdCtx,
                                                                      string queryTypeName,
                                                                      IMethodSymbol stubbedImplMethodSymbol,
                                                                      string stubbedImplMethodName,
                                                                      bool lastArgIsContext)
        {
            // @ToDo: PROPERLY Enforce that a query implementation always returns Task<T> or some T.
            string stubReturnType = null;
            if (stubbedImplMethodSymbol.ReturnsVoid)
            {
                throw new Exception("Query handler must return a value.");
            }
            else
            {
                string implReturnType = stubbedImplMethodSymbol.ReturnType?.ToString() ?? "void";

                // @ToDo: Enforce that a query implementation always returns Task<T> or some T.

                if (implReturnType.Equals("void"))
                {
                    throw new Exception("Query handler must return a value.");
                }
                else if (implReturnType.StartsWith("System.Threading.Tasks.Task<") && implReturnType.EndsWith(">"))
                {
                    stubReturnType = implReturnType.Substring("System.Threading.Tasks.Task<".Length, stubReturnType.Length - "System.Threading.Tasks.Task<".Length - 1);
                }
                else if (implReturnType.Equals("System.Threading.Tasks.Task"))
                {
                    throw new Exception("Query handler must not return a plain Task");
                }
                else
                {
                    stubReturnType = implReturnType;
                }
            }

            ImmutableArray<IParameterSymbol> args = stubbedImplMethodSymbol.Parameters;
            if ((args.Length == 0)
                    || (args.Length == 1 && lastArgIsContext))
            {
                return WorkflowStubTemplateProvider.SingletonInstance.GetExecQuery0ArgSection(stubbedImplMethodName, stubReturnType, queryTypeName);
            }

            if ((args.Length == 1 && !lastArgIsContext)
                    || (args.Length == 2 && lastArgIsContext))
            {
                throw new NotImplementedException("Support for Query Stubs with data input arguments is not yet implemented.");
            }

            // @ToDo: replace with specific error type.
            srcProdCtx.ReportDiagnostic(Diagnostics.Create(
                    Diagnostics.TMPRL003,
                    stubbedImplMethodSymbol.Locations,
                    $"Invalid Query Handler signature."
                  + $" A Query Handler method may have no more than 2 total arguments,"
                  + $" including: 0 or 1 data input arguments, followed by an optional `{WfCtxIface.TypeName}`-argument."
                  + $" However, the method `{stubbedImplMethodName}` is marked with a [{WfQryHndlImplAttr.TypeName}]"
                  + $" and has the following invalid signature: `{GetMethodSignature(stubbedImplMethodSymbol)}`."
                  + $" A stub for this method will NOT be generated."));

            return null;
        }
    }
}


