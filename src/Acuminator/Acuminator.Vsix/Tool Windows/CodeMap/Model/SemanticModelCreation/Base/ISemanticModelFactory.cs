#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;

using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Interface for semantic model factory.
	/// </summary>
	public interface ISemanticModelFactory
	{
		/// <summary>
		/// Try to infer semantic model for <paramref name="typeSymbol"/>. If semantic model can't be inferred the <paramref name="semanticModel"/> is null and the method returns false.
		/// </summary>
		/// <param name="rootSymbol">The root symbol.</param>
		/// <param name="context">The context.</param>
		/// <param name="semanticModel">[out] The inferred semantic model.</param>
		/// <param name="declarationOrder">(Optional) The declaration order of the <see cref="ISemanticModel.Symbol"/>.</param>
		/// <param name="cancellationToken">(Optional) A token that allows processing to be cancelled.</param>
		/// <returns>
		/// True if it succeeds, false if it fails.
		/// </returns>
		bool TryToInferSemanticModel(INamedTypeSymbol rootSymbol, PXContext context, out ISemanticModel? semanticModel,
									 int? declarationOrder = null, CancellationToken cancellationToken = default);
	}
}
