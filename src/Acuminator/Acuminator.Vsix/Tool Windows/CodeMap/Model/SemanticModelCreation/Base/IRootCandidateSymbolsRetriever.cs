using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Semantic;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for root candidate symbols retrieval.
	/// </summary>
	public interface IRootCandidateSymbolsRetriever
	{
		/// <summary>
		/// Get the code map root candidate symbol + node pairs.
		/// </summary>
		/// <param name="treeRoot">The tree root.</param>
		/// <param name="context">The context.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="cancellationToken">(Optional) A token that allows processing to be cancelled.</param>
		/// <returns/>
		IEnumerable<(INamedTypeSymbol Symbol, SyntaxNode Node)> GetCodeMapRootCandidates(SyntaxNode treeRoot, PXContext context,
																						 SemanticModel semanticModel, 
																						 CancellationToken cancellationToken = default);
	}
}
