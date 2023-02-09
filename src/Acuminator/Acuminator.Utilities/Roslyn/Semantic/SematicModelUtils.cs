#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class SematicModelUtils
	{
		/// <summary>
		/// Safely analyse data flow for a <paramref name="node"/> and return <see cref="DataFlowAnalysis"/> if analysis succeeded.
		/// </summary>
		/// <param name="semanticModel">The semanticModel to act on.</param>
		/// <param name="node">The node to analyse.</param>
		/// <returns>
		/// A <see cref="DataFlowAnalysis"/> if the data flow analysis succeeded, <see langword="null"/> if not.
		/// </returns>
		public static DataFlowAnalysis? TryAnalyzeDataFlow(this SemanticModel semanticModel, SyntaxNode node)
		{
			semanticModel.ThrowOnNull(nameof(semanticModel));
			node.ThrowOnNull(nameof(node));

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
		/// Get symbol or first candidate symbol from the <see cref="SemanticModel"/>.
		/// </summary>
		/// <param name="semanticModel">The semanticModel to act on.</param>
		/// <param name="node">The node to retrieve symbol for.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// The symbol or the first candidate symbol.
		/// </returns>
		public static ISymbol? GetSymbolOrFirstCandidate(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellation)
		{
			semanticModel.ThrowOnNull(nameof(semanticModel));
			node.ThrowOnNull(nameof(node));

			var symbolInfo = semanticModel.GetSymbolInfo(node, cancellation);
			return symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
		}

		[SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "Aggregated await is used")]
		public static async Task<(SemanticModel? SemanticModel, SyntaxNode? Root)> GetSemanticModelAndRootAsync(this Document document, 
																												CancellationToken cancellation = default)
		{
			document.ThrowOnNull(nameof(document));

			var semanticModelTask = document.GetSemanticModelAsync(cancellation);
			var syntaxRootTask = document.GetSyntaxRootAsync(cancellation);

			await Task.WhenAll(semanticModelTask, syntaxRootTask).ConfigureAwait(false);

			return (semanticModelTask.Result, syntaxRootTask.Result);
		}
	}
}
