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
		/// Get the code map root candidate symbols.
		/// </summary>
		/// <param name="treeRoot">The tree root.</param>
		/// <param name="context">The context.</param>
		/// <param name="cancellationToken">(Optional) A token that allows processing to be cancelled.</param>
		/// <returns/
		IEnumerable<INamedTypeSymbol> GetCodeMapRootCandidates(SyntaxNode treeRoot, PXContext context, CancellationToken cancellationToken = default);
	}
}
