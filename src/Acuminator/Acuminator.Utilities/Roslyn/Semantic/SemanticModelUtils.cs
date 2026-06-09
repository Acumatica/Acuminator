using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class SemanticModelUtils
	{
		/// <summary>
		/// Safely analyze data flow for a <paramref name="node"/> and return <see cref="DataFlowAnalysis"/> if analysis succeeded.
		/// </summary>
		/// <param name="semanticModel">The semanticModel to act on.</param>
		/// <param name="node">The node to analyze.</param>
		/// <returns>
		/// A <see cref="DataFlowAnalysis"/> if the data flow analysis succeeded, <see langword="null"/> if not.
		/// </returns>
		public static DataFlowAnalysis? TryAnalyzeDataFlow(this SemanticModel semanticModel, SyntaxNode node)
		{
			semanticModel.ThrowOnNull();
			node.ThrowOnNull();

			DataFlowAnalysis? dataFlowAnalysis;

			try
			{
				dataFlowAnalysis = semanticModel.AnalyzeDataFlow(node);
			}
			catch (Exception)
			{
				return null;
			}

			return dataFlowAnalysis?.Succeeded == true
				? dataFlowAnalysis
				: null;
		}

		/// <summary>
		/// Get symbol or best candidate symbol from the <see cref="SemanticModel"/>.
		/// </summary>
		/// <param name="semanticModel">The semanticModel to act on.</param>
		/// <param name="node">The node to retrieve symbol for.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// The symbol or the first candidate symbol.
		/// </returns>
		public static ISymbol? GetSymbolOrBestCandidate(this SemanticModel semanticModel, SyntaxNode node, 
														CancellationToken cancellation)
		{
			node.ThrowOnNull();
			var symbolInfo = semanticModel.CheckIfNull().GetSymbolInfo(node, cancellation);

			// Fast paths
			if (symbolInfo.Symbol != null)
				return symbolInfo.Symbol;
			else if (symbolInfo.CandidateSymbols.Length == 1)
				return symbolInfo.CandidateSymbols[0];
			else if (symbolInfo.CandidateSymbols.IsDefaultOrEmpty ||
					 symbolInfo.CandidateReason is not (CandidateReason.Inaccessible or
														CandidateReason.OverloadResolutionFailure or
														CandidateReason.Ambiguous))
			{
				return null;
			}

			// Try to match symbol with node based on arguments count heuristic
			var argumentList = node.GetArgumentsList();

			if (argumentList == null)
				return symbolInfo.CandidateSymbols.FirstOrDefault();

			return GetBestCandidateHeuristicallyByArgsCount(symbolInfo, argumentList.Arguments.Count);
		}

		private static ISymbol? GetBestCandidateHeuristicallyByArgsCount(in SymbolInfo symbolInfo, int argsCount)
		{
			int minSuitableParametersCount = int.MaxValue;
			ISymbol? heuristicBestCandidate = null;

			foreach (ISymbol candidate in symbolInfo.CandidateSymbols)
			{
				var parameters = candidate.Parameters();

				if (parameters == null)     // symbol doesn't have parameters
					continue;

				int parametersCount = parameters.Value.Length;

				if (argsCount > parametersCount)
					continue;
				else if (argsCount == parametersCount)
					return candidate;									// perfect match
				else if (minSuitableParametersCount > parametersCount)
				{
					// Keep the overload with fewest parameters
					minSuitableParametersCount = parametersCount;
					heuristicBestCandidate = candidate;
				}
			}

			return heuristicBestCandidate ?? symbolInfo.CandidateSymbols.FirstOrDefault();
		}

		[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "Aggregated await is used")]
		public static async Task<(SemanticModel? SemanticModel, SyntaxNode? Root)> GetSemanticModelAndRootAsync(this Document document, 
																												CancellationToken cancellation = default,
																												bool continueOnCapturedContext = false)
		{
			var semanticModelTask = document.CheckIfNull().GetSemanticModelAsync(cancellation);
			var syntaxRootTask = document.GetSyntaxRootAsync(cancellation);

			await Task.WhenAll(semanticModelTask, syntaxRootTask)
					  .ConfigureAwait(continueOnCapturedContext);

			return (semanticModelTask.Result, syntaxRootTask.Result);
		}
	}
}
