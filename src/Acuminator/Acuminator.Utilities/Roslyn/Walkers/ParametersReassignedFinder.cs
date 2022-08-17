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
			
			var dataFlowAnalysis = AnalyseDataFlow(semanticModel, body);

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

		private DataFlowAnalysis? AnalyseDataFlow(SemanticModel semanticModel, CSharpSyntaxNode body)
		{
			DataFlowAnalysis? dataFlowAnalysis;

			try
			{
				dataFlowAnalysis = semanticModel.AnalyzeDataFlow(body);
			}
			catch (Exception)
			{
				return default;
			}

			return dataFlowAnalysis?.Succeeded == true
				? dataFlowAnalysis
				: null;
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
			private SemanticModel? _semanticModel;

			private ExpressionSyntax? _callSite;
			private bool _callSiteIsReached;

			private HashSet<IMethodSymbol>? _checkedLocalMethods;

			[MemberNotNullWhen(returnValue: true, nameof(_callSite))]
			private bool HasCallSite => _callSite != null;

			public HashSet<string>? GetParametersReassignedBeforeCallsite(SyntaxNode declarationNode, ExpressionSyntax? callSite,
																		  IReadOnlyCollection<string> parametersToCheck, SemanticModel semanticModel, 
																		  CancellationToken cancellation)
			{
				if (callSite != null && (callSite.IsMissing || callSite.ContainsDiagnostics))	// Can't analyze syntax with errors
					return null;

				try
				{
					InitializeState(parametersToCheck, callSite, semanticModel, cancellation);

					var body = declarationNode.GetBody();
					body?.Accept(this);

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
				HasCallSite && _callSite.RawKind == expression?.RawKind && _callSite.FullSpan == expression.FullSpan;

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignmentNode)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_callSiteIsReached || assignmentNode.ContainsDiagnostics || assignmentNode.IsMissing)
					return;

				CheckLeftPartOfAssignmentForReassignedParameters(assignmentNode.Left);
				base.VisitAssignmentExpression(assignmentNode);
			}

			private void CheckLeftPartOfAssignmentForReassignedParameters(ExpressionSyntax partOfAssignmentLeftExpression)
			{
				switch (partOfAssignmentLeftExpression)
				{
					case IdentifierNameSyntax identifier
					when _parametersToCheck!.Contains(identifier.Identifier.ValueText):
						AddToReassignedParameters(identifier.Identifier.ValueText);
						break;

					case ElementAccessExpressionSyntax elementAccessNode:
						IdentifierNameSyntax? elementAccessIdentifier = GetIdentifierFromElementAccessNode(elementAccessNode);

						if (elementAccessIdentifier != null && _parametersToCheck!.Contains(elementAccessIdentifier.Identifier.ValueText))
							AddToReassignedParameters(elementAccessIdentifier.Identifier.ValueText);

						break;

					case TupleExpressionSyntax tupleExpression:

						foreach (ArgumentSyntax argument in tupleExpression.Arguments)
							CheckLeftPartOfAssignmentForReassignedParameters(argument.Expression);

						break;
				}
			}

			private IdentifierNameSyntax? GetIdentifierFromElementAccessNode(ElementAccessExpressionSyntax elementAccess)
			{
				ExpressionSyntax current = elementAccess.Expression;

				while (current != null)
				{
					switch (current)
					{
						case IdentifierNameSyntax identifier:
							return identifier;
						case ElementAccessExpressionSyntax outerElementAccess:
							current = outerElementAccess.Expression;
							continue;
						default:
							return null;
					}
				}

				return null;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_callSiteIsReached)
					return;

				// We do nos step into invocation children nodes, just check its arguments for out keywords 
				// and step into local function calls
				CheckIfInvocationReassignesParametersWithOutArguments(invocationExpression);

				// Simple syntax check to filter out invocations that are definitely non local methods
				if (invocationExpression.Expression is not IdentifierNameSyntax)
					return;

				var symbolInfo = _semanticModel.GetSymbolInfo(invocationExpression, _cancellation);
				var localMethod = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;

				// Step into non static local methods since they can reassign parameters.
				// Any outer method parameter used in the local function  
				if (localMethod == null || localMethod.IsStatic || localMethod.MethodKind != MethodKind.LocalFunction)
					return;

				if (_checkedLocalMethods?.Contains(localMethod) == true)
					return;

				AddToCheckedLocalMethods(localMethod);

				// Local method may have parameters with the same name as outer method parameters which will hide the outer method parameters
				var nonRedeclaredParameters = _parametersToCheck.Where(checkedParameterName => localMethod.Parameters.All(p => p.Name != checkedParameterName))
																.ToList(capacity: _parametersToCheck!.Count);
				if (nonRedeclaredParameters.Count == 0)
					return;

				SyntaxNode? localMethodDeclaration = localMethod.GetSyntax(_cancellation);
				var localMethodBody = localMethodDeclaration?.GetBody();

				if (localMethodBody == null)
					return;

				var oldParametersToCheck = _parametersToCheck;

				try
				{		
					_parametersToCheck = nonRedeclaredParameters;
					localMethodBody.Accept(this);
				}
				finally
				{
					_parametersToCheck = oldParametersToCheck;
				}		
			}

			private void CheckIfInvocationReassignesParametersWithOutArguments(InvocationExpressionSyntax invocationExpression)
			{
				if (invocationExpression.ArgumentList.Arguments.Count == 0)
					return;

				for (int i = 0; i < invocationExpression.ArgumentList.Arguments.Count; i++)
				{
					ArgumentSyntax argument = invocationExpression.ArgumentList.Arguments[i];

					if (!argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) || argument.Expression is not IdentifierNameSyntax identifier)
						continue;

					string outArgumentIdentifierName = identifier.Identifier.ValueText;

					if (_parametersToCheck.Contains(outArgumentIdentifierName))
					{
						AddToReassignedParameters(outArgumentIdentifierName);
					}
				}
			}

			public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax localFunction)
			{
				// Local function can be declared in the middle of the code. We don't visit it from the normal tree walking.
				// If there are local function invocations we'll visit its body from the invocation node.
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
			[MemberNotNull(nameof(_parametersToCheck), nameof(_semanticModel))]
			private void InitializeState(IReadOnlyCollection<string> parametersToCheck, ExpressionSyntax? callSite, SemanticModel semanticModel,
										 CancellationToken cancellation)
			{
				_parametersToCheck = parametersToCheck;
				_cancellation = cancellation;
				_callSite = callSite;
				_semanticModel = semanticModel;
			}

			private void ClearState()
			{
				_cancellation = CancellationToken.None;
				_parametersToCheck = null;
				_reassignedParameters = null;
				_callSite = null;
				_callSiteIsReached = false;
				_semanticModel = null;
				_checkedLocalMethods = null;
			}

			[MemberNotNull(nameof(_reassignedParameters))]
			private void AddToReassignedParameters(string parameterName)
			{
				_reassignedParameters = _reassignedParameters ?? new HashSet<string>();
				_reassignedParameters.Add(parameterName);
			}

			[MemberNotNull(nameof(_checkedLocalMethods))]
			private void AddToCheckedLocalMethods(IMethodSymbol localMethod)
			{
				_checkedLocalMethods = _checkedLocalMethods ?? new HashSet<IMethodSymbol>();
				_checkedLocalMethods.Add(localMethod);
			}
			#endregion
		}
	}
}