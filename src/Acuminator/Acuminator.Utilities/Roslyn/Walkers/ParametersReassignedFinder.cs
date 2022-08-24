#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Walkers
{
	/// <summary>
	/// A helper class that finds method parameters that were reassigned inside the method before the specified call site node.
	/// </summary>
	public class ParametersReassignedFinder
	{
		private readonly Walker _reassignSearchingWalker = new Walker();

		/// <summary>
		/// Searches for parameters from the <paramref name="parametersToCheck"/> set to be reassigned before the <paramref name="callSite"/> node. 
		/// The <paramref name="callSite"/> may be <see langword="null"/> to check the whole <paramref name="declarationNode"/>.
		/// </summary>
		/// <param name="declarationNode">
		/// The declaration node. Should be a syntax node which represent something that have parameters and have a body with code.
		/// </param>
		/// <param name="callSite">The call site node. May be <see langword="null"/> to check the whole <paramref name="declarationNode"/>.</param>
		/// <param name="parametersToCheck">The parameter names to check.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// The parameters reassigned before the <paramref name="callSite"/> node.
		/// </returns>
		public HashSet<string>? GetParametersReassignedBeforeCallsite(SyntaxNode declarationNode, ExpressionSyntax? callSite, 
																	  IReadOnlyCollection<string> parametersToCheck, SemanticModel semanticModel,
																	  CancellationToken cancellation)
		{
			declarationNode.ThrowOnNull(nameof(declarationNode));
			parametersToCheck.ThrowOnNull(nameof(parametersToCheck));
			semanticModel.ThrowOnNull(nameof(semanticModel));

			if (callSite != null && !declarationNode.Contains(callSite))
				throw new ArgumentOutOfRangeException(nameof(callSite), "The call site node must be a descendant of the declaration node");

			cancellation.ThrowIfCancellationRequested();

			if (callSite != null && (callSite.IsMissing || callSite.ContainsDiagnostics))   // Can't analyze syntax with errors
				return null;

			if (parametersToCheck.Count == 0)
				return null;

			CSharpSyntaxNode? body = declarationNode.GetBody();

			if (body == null)
				return null;
			
			var dataFlowAnalysis = semanticModel.AnalyseDataFlow(body);

			// If there is no call site then rely only on data flow analysis AlwaysAssigned results, assume that we check the whole containing type member
			if (callSite == null)
			{
				return dataFlowAnalysis != null
					? GetCheckedParametersPresentInDataFlowReassignedSymbols(dataFlowAnalysis.AlwaysAssigned, parametersToCheck)
					: null;
			}

			// If there is a call site we still can't use AlwaysAssigned since assignment may happen after call site
			// But we still can make additional filtering in case dataflow analysis succeeded
			var filteredParametersToCheck = dataFlowAnalysis != null
				? GetCheckedParametersPresentInDataFlowReassignedSymbols(dataFlowAnalysis.WrittenInside, parametersToCheck)
				: parametersToCheck;

			if (filteredParametersToCheck == null)
				return null;

			return _reassignSearchingWalker.GetParametersReassignedBeforeCallsite(body, callSite, parametersToCheck, semanticModel, cancellation);
		}

		private HashSet<string>? GetCheckedParametersPresentInDataFlowReassignedSymbols(ImmutableArray<ISymbol> dataFlowReassigned, 
																						IReadOnlyCollection<string> parametersToCheck)
		{
			if (dataFlowReassigned.IsDefaultOrEmpty)
				return null;

			HashSet<string>? reassignedParameters = null;

			foreach (var parameterName in parametersToCheck)
			{
				if (dataFlowReassigned.Any(symbol => symbol.Name == parameterName))
				{
					reassignedParameters ??= new HashSet<string>();
					reassignedParameters.Add(parameterName);
				}
			}

			return reassignedParameters;
		}

		//----------------------------------------Walker----------------------------------------------
		private class Walker : CSharpSyntaxWalker
		{
			private CancellationToken _cancellation;
			private IReadOnlyCollection<string>? _parametersToCheck;
			private HashSet<string>? _reassignedParameters;
			private List<string>? _reassignedParametersInAssignmentTempStorage;
			private SemanticModel? _semanticModel;

			private SyntaxNode? _body;
			private ExpressionSyntax? _callSite;
			private bool _callSiteIsReached;

			private HashSet<IMethodSymbol>? _checkedLocalFunctions;

			public HashSet<string>? GetParametersReassignedBeforeCallsite(CSharpSyntaxNode body, ExpressionSyntax callSite,
																		  IReadOnlyCollection<string> parametersToCheck, SemanticModel semanticModel, 
																		  CancellationToken cancellation)
			{
				try
				{
					InitializeState(parametersToCheck, callSite, semanticModel, body, cancellation);

					// If call site is an invocation or indexer access we won't reach its arguments during the normal tree walking because we will encounter call first
					// At the same time reassignment may happen in argument expressions. Therefore, we need to check them first
					CheckCallSiteArguments();
					body.Accept(this);

					return _reassignedParameters;
				}
				finally
				{
					// We do not clear the collected _reassignedParameters collection instance here, just reset the stored reference to it to null
					// This does not affect the returned reference to _reassignedParameters in the try block 
					// since for reference types C# returns a copy of a reference to an object by default.
					// 
					// By default C# returns by value and a ref modifier is required to return the reference to the field itself.
					ClearState();
				}
			}

			private void CheckCallSiteArguments()
			{
				var argumentsList = _callSite!.GetArgumentsList();

				if (argumentsList == null || argumentsList.Arguments.Count == 0)
					return;

				foreach (ArgumentSyntax argument in argumentsList.Arguments)
				{
					if (argument.Expression is not (LiteralExpressionSyntax or ThisExpressionSyntax))
					{
						argument.Expression.Accept(this);
					}
				}
			}

			public override void DefaultVisit(SyntaxNode node)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_callSiteIsReached)
					return;
				else if (IsCallSite(node as ExpressionSyntax))
				{
					_callSiteIsReached = true;
					return;
				}

				base.DefaultVisit(node);
			}

			private bool IsCallSite(ExpressionSyntax? expression) =>
				_callSite!.RawKind == expression?.RawKind && _callSite.FullSpan == expression.FullSpan;

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignmentNode)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_callSiteIsReached || assignmentNode.ContainsDiagnostics || assignmentNode.IsMissing)
					return;

				try
				{
					_reassignedParametersInAssignmentTempStorage = null;
					CheckLeftPartOfAssignmentForReassignedParameters(assignmentNode.Left);

					if (_reassignedParametersInAssignmentTempStorage?.Count > 0 && 
						!assignmentNode.Right.Contains(_callSite) &&                    // check for reassignments like: parameter = Call() 
						DoesReassignmentDefinitelyOverrideParameter(assignmentNode))						
					{
						AddToReassignedParameters(_reassignedParametersInAssignmentTempStorage);
					}
				}
				finally
				{
					_reassignedParametersInAssignmentTempStorage = null;
				}
				
				base.VisitAssignmentExpression(assignmentNode);
			}

			private void CheckLeftPartOfAssignmentForReassignedParameters(ExpressionSyntax partOfAssignmentLeftExpression)
			{
				// Here we check the left part of assignment. There could be many different expressions in the left part but we can skip some of them.
				// We are interested in left parts that are:
				// - Identifiers like: parameter = null;
				// - Tuple expressions: (parameter, _) = GetTuple();
				// 
				// It's useful to explicitly state that we don't need:
				// - array or indexer access expressions, in case parameter is an array or a type with an indexer: parameter[0] = null; 
				// - member access expressions, like: parameter.Property = null and parameter?.Property = null;
				//
				// We don't attempt to track the reassignment of the parameter content, we search only for reassignments of the parameter itself
				switch (partOfAssignmentLeftExpression)
				{
					case IdentifierNameSyntax identifier
					when _parametersToCheck!.Contains(identifier.Identifier.ValueText):
						AddToTemporaryListOfReassignedParameters(identifier.Identifier.ValueText);
						return;

					case TupleExpressionSyntax tupleExpression:

						foreach (ArgumentSyntax argument in tupleExpression.Arguments)
							CheckLeftPartOfAssignmentForReassignedParameters(argument.Expression);

						return;
				}
			}

			private void AddToTemporaryListOfReassignedParameters(string parameterName)
			{
				_reassignedParametersInAssignmentTempStorage ??= new List<string>();
				_reassignedParametersInAssignmentTempStorage.Add(parameterName);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_callSiteIsReached)
					return;

				// We do nos step into invocation children nodes, just check its arguments for out keywords 
				// and step into local function calls
				CheckIfInvocationReassignesParametersWithOutArguments(invocationExpression);

				// Simple syntax check to filter out invocations that are definitely non local functions
				if (invocationExpression.Expression is not IdentifierNameSyntax)
					return;

				var symbolInfo = _semanticModel.GetSymbolInfo(invocationExpression, _cancellation);
				var localFunction = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;

				// Analyse local functions since they can reassign parameters from containing methods
				if (localFunction == null || localFunction.MethodKind != MethodKind.LocalFunction)
					return;

				if (_checkedLocalFunctions?.Contains(localFunction) == true)
					return;

				AddToCheckedLocalFunctions(localFunction);

				// Local method may have parameters with the same name as outer method parameters which will hide the outer method parameters
				var nonRedeclaredParameters = localFunction.Parameters.IsDefaultOrEmpty
					? _parametersToCheck
					: _parametersToCheck.Where(checkedParameterName => localFunction.Parameters.All(p => p.Name != checkedParameterName))
										.ToList(capacity: _parametersToCheck!.Count);

				if (nonRedeclaredParameters!.Count == 0)
					return;

				List<ISymbol>? reassignedContainingMethodsParameters = GetReassignedContainingMethodsParameters(localFunction);

				if (reassignedContainingMethodsParameters.IsNullOrEmpty())
					return;

				foreach (string parameterName in nonRedeclaredParameters)
				{
					if (reassignedContainingMethodsParameters.Any(symbol => symbol.Name == parameterName))
					{
						AddToReassignedParameters(parameterName);
					}
				}
			}

			private void CheckIfInvocationReassignesParametersWithOutArguments(InvocationExpressionSyntax invocationExpression)
			{
				if (invocationExpression.ArgumentList.Arguments.Count == 0 || invocationExpression.Equals(_callSite))
					return;

				for (int i = 0; i < invocationExpression.ArgumentList.Arguments.Count; i++)
				{
					ArgumentSyntax argument = invocationExpression.ArgumentList.Arguments[i];

					if (!argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) || argument.Expression is not IdentifierNameSyntax identifier)
						continue;

					string outArgumentIdentifierName = identifier.Identifier.ValueText;

					if (_parametersToCheck.Contains(outArgumentIdentifierName) && DoesReassignmentDefinitelyOverrideParameter(argument))
					{
						AddToReassignedParameters(outArgumentIdentifierName);
					}
				}
			}

			private List<ISymbol>? GetReassignedContainingMethodsParameters(IMethodSymbol localFunction)
			{
				SyntaxNode? localFunctionDeclaration = localFunction.GetSyntax(_cancellation);
				var localFunctionBody = localFunctionDeclaration?.GetBody();

				// Static local functions can't reassign parameters from containing methods
				if (localFunctionBody == null || localFunction.IsDefinitelyStatic(localFunctionDeclaration!))
					return null;

				// If there are containing local functions we must check for the first containing static local function.
				// Only its parameters and parameters of its local functions can be reassigned by this localFunction
				var containingMethodsWithReassignableParameters = localFunction.GetContainingMethods()
																			   .TakeWhile(function => function.MethodKind != MethodKind.LocalFunction ||
																									  !function.IsDefinitelyStatic(_cancellation))
																			   .ToList(capacity: 1);
				if (containingMethodsWithReassignableParameters.Count == 0)
					return null;

				var localMethodDFA = _semanticModel!.AnalyseDataFlow(localFunctionBody);

				if (localMethodDFA == null || localMethodDFA.AlwaysAssigned.IsDefaultOrEmpty)
					return null;

				return localMethodDFA.AlwaysAssigned
									 .Where(symbol => containingMethodsWithReassignableParameters.Contains(symbol.ContainingSymbol))
									 .ToList();
			}

			public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax localFunction)
			{
				// Local function can be declared in the middle of the code. We don't visit it from the normal tree walking.
				// If there are local function invocations we'll analyze data flow for its body obtained from the invocation
			}

			private bool DoesReassignmentDefinitelyOverrideParameter(SyntaxNode reassignmentNode)
			{
				// Check if reassignment is in arguments
				if (CallSiteArgumentsContainReassignment(reassignmentNode))
					return true;

				var reassignStatementParent = reassignmentNode.GetStatementNode()?.Parent;
				return reassignStatementParent?.Contains(_callSite) ?? false;
			}

			private bool CallSiteArgumentsContainReassignment(SyntaxNode reassignmentNode)
			{
				var argumentsList = _callSite?.GetArgumentsList();
				return argumentsList?.Contains(reassignmentNode) ?? false;
			}

			#region Skip visiting anonymous functions and lambdas
			// Lambdas and amomymous functions can be declared in the middle of the code. 
			// We don't visit them from the normal tree walking since their declaration is not a running code that can reassign something
			// Also, currently analysis of invocations of lambdas is not supported
			public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax anonymousMethodExpression)
			{

			}

			public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax simpleLambdaExpression)
			{

			}
			public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax  parenthesizedLambdaExpression)
			{

			}
			#endregion

			#region State Management
			[MemberNotNull(nameof(_parametersToCheck), nameof(_semanticModel), nameof(_callSite), nameof(_body))]
			private void InitializeState(IReadOnlyCollection<string> parametersToCheck, ExpressionSyntax callSite, SemanticModel semanticModel,
										 SyntaxNode body, CancellationToken cancellation)
			{
				_parametersToCheck = parametersToCheck;
				_cancellation = cancellation;
				_callSite = callSite;
				_body = body;
				_semanticModel = semanticModel;
			}

			private void ClearState()
			{
				_cancellation = CancellationToken.None;
				_parametersToCheck = null;
				_reassignedParameters = null;
				_callSite = null;
				_body = null;
				_callSiteIsReached = false;
				_semanticModel = null;
				_checkedLocalFunctions = null;
			}

			[MemberNotNull(nameof(_reassignedParameters))]
			private void AddToReassignedParameters(string parameterName)
			{
				_reassignedParameters = _reassignedParameters ?? new HashSet<string>();
				_reassignedParameters.Add(parameterName);
			}

			[MemberNotNull(nameof(_reassignedParameters))]
			private void AddToReassignedParameters(IEnumerable<string> parametersNames)
			{
				_reassignedParameters = _reassignedParameters ?? new HashSet<string>();

				foreach (var parameterName in parametersNames)
				{
					_reassignedParameters.Add(parameterName);
				}
			}

			[MemberNotNull(nameof(_checkedLocalFunctions))]
			private void AddToCheckedLocalFunctions(IMethodSymbol localFunction)
			{
				_checkedLocalFunctions = _checkedLocalFunctions ?? new HashSet<IMethodSymbol>();
				_checkedLocalFunctions.Add(localFunction);
			}
			#endregion
		}
	}
}