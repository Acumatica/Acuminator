#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.ArgumentsToParametersMapping;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Walkers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	public partial class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
	{
		private class LongOperationsChecker : NestedInvocationWalker
		{
			private readonly SyntaxNodeAnalysisContext _context;
			private readonly Stack<PassedParametersToNotBeCaptured?> _nonCapturablePassedParameters = new();
			private readonly ParametersReassignedFinder _parametersReassignedFinder = new();
			private readonly NonCapturableElementsInArgumentsFinder _nonCapturableElementsInArgumentsFinder;

			public LongOperationsChecker(SyntaxNodeAnalysisContext context, PXContext pxContext) :
									base(pxContext, context.CancellationToken)
			{
				_context = context;
				_nonCapturableElementsInArgumentsFinder = new NonCapturableElementsInArgumentsFinder(CancellationToken, PxContext);
			}

			public void CheckForCapturedGraphReferencesInDelegateClosures(TypeDeclarationSyntax typeDeclarationNode)
			{
				ThrowIfCancellationRequested();

				typeDeclarationNode.Accept(this);
			}

			#region Visitor Optimization - do not visit some subtrees
			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node) { }

			public override void VisitEnumDeclaration(EnumDeclarationSyntax node) { }

			public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node) { }

			public override void VisitXmlComment(XmlCommentSyntax node) { }
			#endregion

			#region Visiting invocations and checking long run delegates 
			public override void VisitInvocationExpression(InvocationExpressionSyntax longOperationSetupMethodInvocationNode) =>
				AnalyzeLongOperationDelegate(longOperationSetupMethodInvocationNode);

			private void AnalyzeLongOperationDelegate(InvocationExpressionSyntax longOperationSetupMethodInvocationNode)
			{
				ThrowIfCancellationRequested();

				SemanticModel? semanticModel = GetSemanticModel(longOperationSetupMethodInvocationNode.SyntaxTree);

				if (semanticModel == null)
				{
					base.VisitInvocationExpression(longOperationSetupMethodInvocationNode);
					return;
				}

				var longOperationDelegateType = LongOperationDelegateTypeClassifier.GetLongOperationDelegateType(longOperationSetupMethodInvocationNode,
																												 semanticModel, PxContext, CancellationToken);
				switch (longOperationDelegateType)
				{
					case LongOperationDelegateType.ProcessingDelegate:
						AnalyzeSetProcessDelegateMethod(semanticModel, longOperationSetupMethodInvocationNode);
						return;

					case LongOperationDelegateType.LongRunDelegate:
						AnalyzeStartOperationDelegateMethod(semanticModel, longOperationSetupMethodInvocationNode);
						return;

					default:
						base.VisitInvocationExpression(longOperationSetupMethodInvocationNode);
						return;
				}
			}

			private void AnalyzeSetProcessDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax setDelegateInvocation)
			{
				if (setDelegateInvocation.ArgumentList.Arguments.Count == 0)
					return;

				ExpressionSyntax processingDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[0].Expression;
				bool isMainProcessingDelegateCorrect = CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, processingDelegateParameter,
																					  LongOperationDelegateType.ProcessingDelegate);

				if (isMainProcessingDelegateCorrect && setDelegateInvocation.ArgumentList.Arguments.Count > 1)
				{
					ExpressionSyntax finallyHandlerDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[1].Expression;
					CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, finallyHandlerDelegateParameter,
												   LongOperationDelegateType.ProcessingDelegate);
				}
			}

			private void AnalyzeStartOperationDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax startOperationInvocation)
			{
				if (startOperationInvocation.ArgumentList.Arguments.Count < 2)
					return;

				ExpressionSyntax longRunDelegateParameter = startOperationInvocation.ArgumentList.Arguments[1].Expression;
				CheckDataFlowForDelegateMethod(semanticModel, startOperationInvocation, longRunDelegateParameter,
											   LongOperationDelegateType.LongRunDelegate);
			}

			private bool CheckDataFlowForDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax longOperationSetupMethodInvocationNode,
														ExpressionSyntax longOperationDelegateNode, LongOperationDelegateType delegateType)
			{
				ThrowIfCancellationRequested();

				PassedParametersToNotBeCaptured? nonCapturableParametersOfMethodContainingCallSite = _nonCapturablePassedParameters.Peek();
				var capturedLocalInstancesInExpressionsChecker =
					new CapturedLocalInstancesInExpressionsChecker(nonCapturableParametersOfMethodContainingCallSite, semanticModel, PxContext,
																   CancellationToken);

				var capturedInstanceType = capturedLocalInstancesInExpressionsChecker.ExpressionCapturesLocalIntanceInClosure(longOperationDelegateNode);

				if (capturedInstanceType.HasValue)
				{
					string capturedInstanceTypeName = capturedInstanceType == CapturedInstanceType.PXGraph
						? Resources.PX1008Title_CapturedGraphFormatArg
						: Resources.PX1008Title_CapturedPXAdapterFormatArg;

					string delegateTypeName = delegateType == LongOperationDelegateType.ProcessingDelegate
						? Resources.PX1008Title_ProcessingDelegateFormatArg
						: Resources.PX1008Title_LongRunDelegateFormatArg;

					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode,
									 capturedInstanceTypeName, delegateTypeName);
					return false;
				}

				return true;
			}
			#endregion

			#region Recursive method calls visiting - filtering visited methods and perform data flow analysis
			protected override void BeforeBypassCheck(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite)
			{
				base.BeforeBypassCheck(calledMethod, calledMethodNode, callSite);

				PassedParametersToNotBeCaptured? nonCapturableParameters = GetMethodNonCapturableParameters(calledMethod, callSite);

				_nonCapturablePassedParameters.Push(nonCapturableParameters);
			}

			protected override bool BypassMethod(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite)
			{
				if (base.BypassMethod(calledMethod, calledMethodNode, callSite))
					return true;

				PassedParametersToNotBeCaptured? calledMethodNonCapturableParameters = _nonCapturablePassedParameters.Count > 0
					? _nonCapturablePassedParameters.Peek()
					: null;

				// If the called method has some non-capturable method parameters then we need to step into it
				if (calledMethodNonCapturableParameters?.PassedInstancesCount > 0)
					return false;

				// Otherwise, we need to step only into non-static methods of graphs/graph extensions since they can capture "this" reference
				return calledMethod.IsStatic || calledMethod.ContainingType == null || !calledMethod.ContainingType.IsPXGraphOrExtension(PxContext);
			}

			protected override void AfterRecursiveVisit(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite, bool wasVisited)
			{
				base.AfterRecursiveVisit(calledMethod, calledMethodNode, callSite, wasVisited);

				_nonCapturablePassedParameters.Pop();
			}

			private PassedParametersToNotBeCaptured? GetMethodNonCapturableParameters(IMethodSymbol calledMethod, ExpressionSyntax callSite)
			{
				if (calledMethod.Parameters.IsDefaultOrEmpty)
					return null;                                //Method does not have any parameters so we can't pass anything to it that can be captured later

				var argumentsList = callSite.GetArgumentsList();

				if (argumentsList == null || argumentsList.Arguments.Count == 0)
					return null;

				var callingTypeMemberNode = callSite.Parent<MemberDeclarationSyntax>();

				if (callingTypeMemberNode == null)
					return null;

				SemanticModel? semanticModel = GetSemanticModel(callSite.SyntaxTree);
				ISymbol? callingTypeMember = semanticModel?.GetDeclaredSymbol(callingTypeMemberNode, CancellationToken);

				if (callingTypeMember?.ContainingType == null)
					return null;

				var nonCapturableParameters = GetNonCapturableParameterNames(callingTypeMember, callingTypeMemberNode, argumentsList, 
																			 callSite, semanticModel!, calledMethod);
				if (nonCapturableParameters.IsNullOrEmpty())
					return null;

				return new PassedParametersToNotBeCaptured(nonCapturableParameters);
			}

			private HashSet<string>? GetNonCapturableParameterNames(ISymbol callingTypeMember, MemberDeclarationSyntax callingTypeMemberNode,
																	BaseArgumentListSyntax argumentsList, ExpressionSyntax callSite, 
																	SemanticModel semanticModel, IMethodSymbol calledMethod)
			{
				ThrowIfCancellationRequested();

				// There can be three types of non capturable parameters that can be passed to other methods:
				// 1. PXAdapter from action delegate
				// 2. this reference if we are in a graph or graph extension
				// 3. non capturable parameters passed to the calling methods from the previous method call
				
				IParameterSymbol? adapterParameter = GetNonCapturableAdapterParameter(callingTypeMember as IMethodSymbol, argumentsList.Arguments);

				NonCapturableArgumentsInfo? nonCapturableArguments = GetNonCapturableArgumentsInfo(argumentsList.Arguments, callingTypeMember, 
																								   adapterParameter);
				if (nonCapturableArguments.IsNullOrEmpty())
					return null;

				FilterReassignedParameters(nonCapturableArguments, callingTypeMemberNode, callSite, semanticModel);

				if (nonCapturableArguments.Count == 0)
					return null;

				ArgumentsToParametersMapping? argumentsToParametersMapping = calledMethod.MapArgumentsToParameters(argumentsList);

				if (argumentsToParametersMapping == null)
					return null;

				var nonCapturableParametersOfCalledMethod = new HashSet<string>();

				foreach (NonCapturableArgument argument in nonCapturableArguments)
				{
					var mappedParameter = argumentsToParametersMapping.Value.GetMappedParameter(calledMethod, argument.Index);
					nonCapturableParametersOfCalledMethod.Add(mappedParameter.Name);
				}

				return nonCapturableParametersOfCalledMethod;
			}

			/// <summary>
			/// Attempts to get a non-capturable adapter parameter from the <paramref name="callingMethod"/>
			/// if the calling method is an action handler with an adapter parameter.
			/// </summary>
			/// <remarks>
			/// This method checks the calling method to see if it could be an action handler with PXAdapter adapter parameter.
			/// Adapters from action handlers can't be captured by long run delegates' closures.
			/// <br/><br/>
			/// When we check the method to see if it can be an action handler we must take into account that actions can be added to graph dynamically.
			/// This means that graphs and graph extensions are not only places declaring action handlers.<br/>
			/// For example, actions can be added in custom attributes and views. Thus, we can't make any assumptions about the method's containing type.<br/>
			/// Therefore, we will consider a method to be an action handler if it has a proper signature and a PXButton attribute declared on it or on base methods in case of overrides.
			/// </remarks>
			/// <param name="callingMethod">The method containing the call.</param>
			/// <param name="callArguments">The method call arguments</param>
			/// <returns>
			/// The adapter parameter or null.
			/// </returns>
			private IParameterSymbol? GetNonCapturableAdapterParameter(IMethodSymbol? callingMethod, SeparatedSyntaxList<ArgumentSyntax> callArguments)
			{
				if (callingMethod?.ContainingType == null || callingMethod.Parameters.IsDefaultOrEmpty || !callingMethod.IsValidActionHandler(PxContext))
					return null;

				var adapterParameter = callingMethod.Parameters[0];

				// Check for PXButton attribute
				if (callingMethod.HasAttribute(PxContext.AttributeTypes.PXButtonAttribute, checkOverrides: true))
					return adapterParameter;

				// Check for action handlers that are declared inside the current graph but are not marked with attribute
				INamedTypeSymbol containingType = callingMethod.ContainingType;
				bool isInsideGraph              = containingType.IsPXGraph(PxContext);
				bool isInsideGraphExtension     = containingType.IsPXGraphExtension(PxContext);

				// If method is outside graph or graph extension then assume that it's not an action handler
				if (!isInsideGraph && !isInsideGraphExtension)
					return null;

				// Finally check if the method is a PXOverride of the action handler and has a corresponding action.
				if (!callingMethod.HasAttribute(PxContext.AttributeTypes.PXOverrideAttribute, checkOverrides: true))
					return null;

				IEnumerable<ITypeSymbol> typesToCheck = isInsideGraph
					? containingType.GetGraphWithBaseTypes()
					: containingType.GetGraphExtensionWithBaseExtensions(PxContext, SortDirection.Descending, includeGraph: true);

				bool hasCorrespondingAction = typesToCheck.SelectMany(type => type.GetMembers().OfType<IFieldSymbol>())
														  .Any(field => field.Name.Equals(callingMethod.Name, StringComparison.OrdinalIgnoreCase) &&
																		field.Type.IsPXAction(PxContext));	
				return hasCorrespondingAction
					? adapterParameter
					: null;
			}

			private NonCapturableArgumentsInfo? GetNonCapturableArgumentsInfo(SeparatedSyntaxList<ArgumentSyntax> callArguments, ISymbol callingTypeMember, 
																			  IParameterSymbol? adapterParameter)
			{
				var parametersPassedBefore = GetAllPassedParameterNames(adapterParameter?.Name);

				if (parametersPassedBefore == null || parametersPassedBefore.Count == 0)
					return default;

				NonCapturableArgumentsInfo? nonCapturableArgumentsInfo = null;

				for (int argIndex = 0; argIndex < callArguments.Count; argIndex++)
				{
					ArgumentSyntax argument = callArguments[argIndex];

					var (captureLocalGraphInstance, parametersUsedInArgument) = 
						_nonCapturableElementsInArgumentsFinder.GetElementsUsedInArgumentExpression(argument, callingTypeMember, parametersPassedBefore);
					bool captureNonCapturableElement = captureLocalGraphInstance || parametersUsedInArgument?.Count > 0;

					if (captureNonCapturableElement)
					{
						nonCapturableArgumentsInfo ??= new NonCapturableArgumentsInfo();
						var nonCapturableArgument = new NonCapturableArgument(argIndex, captureLocalGraphInstance, parametersUsedInArgument);

						nonCapturableArgumentsInfo.Add(nonCapturableArgument);
					}
				}

				return nonCapturableArgumentsInfo;				
			}

			private ICollection<string>? GetAllPassedParameterNames(string? adapterParameterName)
			{
				PassedParametersToNotBeCaptured? parametersPassedBefore = _nonCapturablePassedParameters.Count > 0
					? _nonCapturablePassedParameters.Peek()
					: null;

				if (adapterParameterName == null)
					return parametersPassedBefore;
				else if (parametersPassedBefore == null)
					return new List<string> { adapterParameterName };
				else
				{
					var passedParametersNames = parametersPassedBefore.ToList(capacity: parametersPassedBefore.PassedInstancesCount + 1);
					passedParametersNames.Add(adapterParameterName);

					return passedParametersNames;
				}
			}

			private void FilterReassignedParameters(NonCapturableArgumentsInfo nonCapturableArguments, MemberDeclarationSyntax callingTypeMemberNode,
													ExpressionSyntax callSite, SemanticModel semanticModel)
			{
				if (!nonCapturableArguments.HasNonCapturableParameters)
					return;

				var reassignedParameters =
					_parametersReassignedFinder.GetParametersReassignedBeforeCallsite(callingTypeMemberNode, callSite,
																					  nonCapturableArguments.UsedParameters,
																					  semanticModel, CancellationToken);
				if (reassignedParameters.IsNullOrEmpty())
					return;

				foreach (string reassignedParameter in reassignedParameters)
				{
					nonCapturableArguments.RemoveParameterUsageFromArguments(reassignedParameter);
				}
			}
			#endregion
		}
	}
}