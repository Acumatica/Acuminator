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
		/// The symbol or the best candidate symbol.
		/// </returns>
		/// <remarks>
		/// The best candidate symbols is determined heuristically based on arguments count in case of method group or overloaded method invocation.<br/>
		/// The candidate with the closest match in terms of arguments count is selected as the best candidate.
		/// </remarks>
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

				if (parameters == null)			// symbol doesn't have parameters
					continue;

				int parametersCount = parameters.Value.Length;
				bool hasParamsParameter = parametersCount > 0 && parameters.Value[parametersCount - 1].IsParams;

				if (!hasParamsParameter)
				{
					// perfect match heuristic: the same number of arguments and parameters and no params parameter
					if (argsCount == parametersCount)
						return candidate;
					else if (argsCount > parametersCount)
					{
						// Filtering out candidates with too few parameters when the invocation has more arguments than parameters,
						// but only if there is no params parameter that can match extra arguments
						continue;
					} 
				}

				int optionalCount = parameters.Value.Count(p => p.IsOptional);
				int minRequiredParametersCount = hasParamsParameter
					? parametersCount - optionalCount - 1       // params parameter can match 0 or more arguments, so it's not required
					: parametersCount - optionalCount;

				// If the invocation has fewer arguments than required parameters, then this candidate is not a good match
				if (argsCount < minRequiredParametersCount)
					continue;

				if (minSuitableParametersCount > parametersCount)
				{
					// Keep the overload with fewest parameters
					// In theory, we could specify parametersCount - 1 here for candidate with params parameter, 
					// but in C# overload resolution method with params parameter is considered less specific 
					// than the same method without params parameter, so we can keep the current heuristic simple
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
