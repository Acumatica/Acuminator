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
			private readonly Stack<PassedParametersToNotBeCaptured?> _nonCapturablePassedParameters = new Stack<PassedParametersToNotBeCaptured?>();

			public LongOperationsChecker(SyntaxNodeAnalysisContext context, PXContext pxContext) :
									base(pxContext, context.CancellationToken)
			{
				_context = context;
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
				bool isMainProcessingDelegateCorrect = CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, processingDelegateParameter);

				if (isMainProcessingDelegateCorrect && setDelegateInvocation.ArgumentList.Arguments.Count > 1)
				{
					ExpressionSyntax finallyHandlerDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[1].Expression;
					CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, finallyHandlerDelegateParameter);
				}
			}

			private void AnalyzeStartOperationDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax startOperationInvocation)
			{
				if (startOperationInvocation.ArgumentList.Arguments.Count < 2)
					return;

				ExpressionSyntax longRunDelegateParameter = startOperationInvocation.ArgumentList.Arguments[1].Expression;
				CheckDataFlowForDelegateMethod(semanticModel, startOperationInvocation, longRunDelegateParameter);
			}

			private bool CheckDataFlowForDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax longOperationSetupMethodInvocationNode,
														ExpressionSyntax longOperationDelegateNode)
			{
				ThrowIfCancellationRequested();

				PassedParametersToNotBeCaptured? nonCapturableParametersOfMethodContainingCallSite = _nonCapturablePassedParameters.Peek();
				var capturedLocalInstancesInExpressionsChecker =
					new CapturedLocalInstancesInExpressionsChecker(nonCapturableParametersOfMethodContainingCallSite, semanticModel, PxContext,
																   CancellationToken);

				if (capturedLocalInstancesInExpressionsChecker.ExpressionCapturesLocalIntanceInClosure(longOperationDelegateNode))
				{
					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode);
					return false;
				}

				return true;
			}

			protected override void BeforeBypassCheck(IMethodSymbol calledMethod, CSharpSyntaxNode calledMethodNode, ExpressionSyntax callSite)
			{
				base.BeforeBypassCheck(calledMethod, calledMethodNode, callSite);

				PassedParametersToNotBeCaptured? nonCapturableParameters = GetMethodNonCapturableParameters(calledMethod, callSite);

				_nonCapturablePassedParameters.Push(nonCapturableParameters);
			}

			protected override bool BypassMethod(IMethodSymbol methodSymbol, CSharpSyntaxNode methodNode, ExpressionSyntax callSite)
			{
				if (base.BypassMethod(methodSymbol, methodNode, callSite) || methodSymbol.IsStatic || _nonCapturablePassedParameters.Count == 0)
					return true;

				PassedParametersToNotBeCaptured? currentNonCapturableParameters = _nonCapturablePassedParameters.Peek();
				return currentNonCapturableParameters?.PassedInstancesCount > 0;
			}

			protected override void AfterRecursiveVisit(IMethodSymbol methodSymbol, CSharpSyntaxNode methodNode, ExpressionSyntax callSite, bool wasVisited)
			{
				base.AfterRecursiveVisit(methodSymbol, methodNode, callSite, wasVisited);

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

				var nonCapturableParameters = GetNonCapturableParameterNames(callingTypeMember, argumentsList, semanticModel!, calledMethod);

				if (nonCapturableParameters == null)
					return null;

				return new PassedParametersToNotBeCaptured(nonCapturableParameters);
			}

			private HashSet<string>? GetNonCapturableParameterNames(ISymbol callingTypeMember, BaseArgumentListSyntax argumentsList,
																	SemanticModel semanticModel, IMethodSymbol calledMethod)
			{
				ThrowIfCancellationRequested();

				// There can be three types of non capturable parameters that can be passed to other methods:
				// 1. PXAdapter from action delegate
				// 2. this reference if we are in a graph or graph extension
				// 3. non capturable parameters passed to the calling methods from the previous method call
				HashSet<string>? passedNonCapturableParameters = null;
				var nonCapturableAdapterParameterIndexes =
					TryGetNonCapturableAdapterParameterIndexesInCallArguments(callingTypeMember as IMethodSymbol, 
																			  semanticModel, argumentsList.Arguments);
				ArgumentsToParametersMapping? argumentsToParametersMapping = null;

				// Add to non capturables the parameters from the called method that accepted the adapter from the action handler
				if (!TryAddNonCapturableParameters(nonCapturableAdapterParameterIndexes))
					return null;

				// Add to non capturables the parameters from the called method that accepted this reference 
				// or non capturable parameters from the previous method call
				var otherNonCapturableArgumentsIndexes = GetNonCapturableArgumentsIndexes(argumentsList.Arguments, callingTypeMember);

				if (!TryAddNonCapturableParameters(otherNonCapturableArgumentsIndexes))
					return null;

				return passedNonCapturableParameters;

				//--------------------------------------------Local Function------------------------------------------------------
				bool TryAddNonCapturableParameters(List<int>? nonCapturableArgumentsIndexesToAdd)
				{
					if (nonCapturableArgumentsIndexesToAdd.IsNullOrEmpty())
						return true;

					argumentsToParametersMapping ??= calledMethod.MapArgumentsToParameters(argumentsList);

					if (argumentsToParametersMapping == null)
						return false;

					passedNonCapturableParameters ??= new HashSet<string>();

					foreach (int adapterArgumentIndex in nonCapturableArgumentsIndexesToAdd)
					{
						var mappedParameter = argumentsToParametersMapping.Value.GetMappedParameter(calledMethod, adapterArgumentIndex);
						passedNonCapturableParameters.Add(mappedParameter.Name);
					}

					return true;
				}
			}

			/// <summary>
			/// Attempts to get indexes in <paramref name="callArguments"/> of a non-capturable adapter parameter from the <paramref name="callingMethod"/>
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
			/// <param name="semanticModel">The semantic model.</param>
			/// <param name="callArguments">The method call arguments</param>
			/// <returns>
			/// A list of call argument indexes that are using the adapter parameter.
			/// Returns <see langword="null"/> if there is no such parameter in the calling method or it is not present in call arguments.
			/// </returns>
			private List<int>? TryGetNonCapturableAdapterParameterIndexesInCallArguments(IMethodSymbol? callingMethod, SemanticModel semanticModel, 
																						 SeparatedSyntaxList<ArgumentSyntax> callArguments)
			{
				if (callingMethod?.ContainingType == null || callingMethod.Parameters.IsDefaultOrEmpty || !callingMethod.IsValidActionHandler(PxContext))
					return null;

				var adapterParameter = callingMethod.Parameters[0];

				// Check that adapter is passed among the method call arguments
				List<int>? adapterIndexes = GetAdapterIndexesInCallArguments(adapterParameter.Name, callArguments);

				if (adapterIndexes.IsNullOrEmpty())
					return null;

				if (callingMethod.HasAttribute(PxContext.AttributeTypes.PXButtonAttribute, checkOverrides: true))
					return adapterIndexes;

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
					? adapterIndexes
					: null;
			}

			private List<int>? GetAdapterIndexesInCallArguments(string adapterParameterName, SeparatedSyntaxList<ArgumentSyntax> callArguments)
			{
				List<int>? adapterIndexes = null;

				for (int argIndex = 0; argIndex < callArguments.Count; argIndex++)
				{
					ArgumentSyntax argument = callArguments[argIndex];

					if (argument.Expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == adapterParameterName)
					{
						adapterIndexes = adapterIndexes ?? new List<int>();
						adapterIndexes.Add(argIndex);
					}
				}

				return adapterIndexes;
			}

			private List<int>? GetNonCapturableArgumentsIndexes(SeparatedSyntaxList<ArgumentSyntax> callArguments, ISymbol callingTypeMember)
			{
				PassedParametersToNotBeCaptured? parametersPassedBefore = _nonCapturablePassedParameters.Count > 0
					? _nonCapturablePassedParameters.Peek()
					: null;

				List<int>? nonCapturableArguments = null;

				for (int argIndex = 0; argIndex < callArguments.Count; argIndex++)
				{
					ArgumentSyntax argument = callArguments[argIndex];

					if (ExpressionContainsNonCapturableElement(argument.Expression, callingTypeMember, parametersPassedBefore, recursionDepth: 0))
					{
						nonCapturableArguments = nonCapturableArguments ?? new List<int>();
						nonCapturableArguments.Add(argIndex);
					}
				}

				return nonCapturableArguments;
			}

			private bool ExpressionContainsNonCapturableElement(ExpressionSyntax expression, ISymbol callingTypeMember,
																PassedParametersToNotBeCaptured? parametersPassedBefore, int recursionDepth)
			{
				const int maxDepth = 100;

				if (recursionDepth < maxDepth)
					return false;

				switch (expression)
				{
					case IdentifierNameSyntax identifierName
					when parametersPassedBefore != null && parametersPassedBefore.PassedInstancesCount > 0:
						return parametersPassedBefore.Contains(identifierName.Identifier.ValueText);

					case ThisExpressionSyntax thisExpression
					when callingTypeMember.ContainingType.IsPXGraphExtension(PxContext):
						return true;

					case InitializerExpressionSyntax initializer:
						return initializer.Expressions.Any(expr => ExpressionContainsNonCapturableElement(expr, callingTypeMember,
																										  parametersPassedBefore, recursionDepth + 1));
					case TupleExpressionSyntax tupleExpression:
						return tupleExpression.Arguments.Any(argument => ExpressionContainsNonCapturableElement(argument.Expression, callingTypeMember,
																												parametersPassedBefore, recursionDepth + 1));
					case ElementAccessExpressionSyntax elementAccessExpression
					when parametersPassedBefore != null && parametersPassedBefore.PassedInstancesCount > 0:
						return elementAccessExpression.Expression is IdentifierNameSyntax elementAccessIdentifier
							? parametersPassedBefore.Contains(elementAccessIdentifier.Identifier.ValueText)
							: ExpressionContainsNonCapturableElement(elementAccessExpression.Expression, callingTypeMember, parametersPassedBefore, recursionDepth + 1);

					case ConditionalExpressionSyntax conditionalExpressionSyntax:
						return ExpressionContainsNonCapturableElement(conditionalExpressionSyntax.WhenTrue, callingTypeMember, parametersPassedBefore, recursionDepth + 1) ||
							   ExpressionContainsNonCapturableElement(conditionalExpressionSyntax.WhenFalse, callingTypeMember, parametersPassedBefore, recursionDepth + 1);

					case AssignmentExpressionSyntax assignmentExpression:
						return ExpressionContainsNonCapturableElement(assignmentExpression.Right, callingTypeMember, parametersPassedBefore, recursionDepth + 1);

					case ParenthesizedExpressionSyntax parenthesizedExpression:
						return ExpressionContainsNonCapturableElement(parenthesizedExpression.Expression, callingTypeMember, parametersPassedBefore, recursionDepth + 1);

					case BinaryExpressionSyntax binaryExpression
					when binaryExpression.IsKind(SyntaxKind.CoalesceExpression):
						return ExpressionContainsNonCapturableElement(binaryExpression.Left, callingTypeMember, parametersPassedBefore, recursionDepth + 1) ||
							   ExpressionContainsNonCapturableElement(binaryExpression.Right, callingTypeMember, parametersPassedBefore, recursionDepth + 1);

					case InvocationExpressionSyntax invocationExpression:
					case MemberAccessExpressionSyntax memberAccessExpression:
					case ConditionalAccessExpressionSyntax conditionalAccessExpression:
					case BinaryExpressionSyntax binaryExpression:
					case PostfixUnaryExpressionSyntax postfixUnaryExpression:
					case PrefixUnaryExpressionSyntax prefixUnaryExpression:
					case TypeOfExpressionSyntax typeOfExpression:
						return false;

					default:
						foreach (SyntaxNode childNode in expression.ChildNodes())
						{
							if (childNode is ExpressionSyntax childExpression &&
								ExpressionContainsNonCapturableElement(childExpression, callingTypeMember, parametersPassedBefore, recursionDepth + 1))
							{
								return true;
							}

							var innerExpression = childNode.DescendantNodes().OfType<ExpressionSyntax>().FirstOrDefault();

							if (innerExpression != null &&
							    ExpressionContainsNonCapturableElement(innerExpression, callingTypeMember, parametersPassedBefore, recursionDepth + 1))
							{
								return true;
							}	
						}

						return false;
				}
			}
		}
	}
}