#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Default implementation for <see cref="ISemanticModelFactory"/> interface.
	/// </summary>
	public class SemanticModelFactoryDefault : ISemanticModelFactory
	{
		/// <summary>
		/// Try to infer semantic model for <paramref name="rootSymbol"/>. If semantic model can't be inferred
		/// the <paramref name="semanticModel"/> is null and the method returns false.
		/// </summary>
		/// <param name="rootSymbol">The root symbol.</param>
		/// <param name="rootNode">The root node.</param>
		/// <param name="context">The context.</param>
		/// <param name="semanticModel">[out] The inferred semantic model.</param>
		/// <param name="declarationOrder">(Optional) The declaration order of the <see cref="ISemanticModel.Symbol"/>.</param>
		/// <param name="cancellationToken">(Optional) A token that allows processing to be cancelled.</param>
		/// <returns>
		/// True if it succeeds, false if it fails.
		/// </returns>
		public virtual bool TryToInferSemanticModel(INamedTypeSymbol rootSymbol, SyntaxNode rootNode, PXContext context, out ISemanticModel? semanticModel,
													int? declarationOrder = null, CancellationToken cancellationToken = default)
		{
			rootSymbol.ThrowOnNull();
			context.ThrowOnNull();
			cancellationToken.ThrowIfCancellationRequested();

			if (rootSymbol.IsPXGraphOrExtension(context))
			{
				return TryToInferGraphOrGraphExtensionSemanticModel(rootSymbol, context, GraphSemanticModelCreationOptions.CollectGeneralGraphInfo,
																	out semanticModel, cancellationToken);
			}
			else if (rootSymbol.IsDacOrExtension(context))
			{
				return TryToInferDacOrDacExtensionSemanticModel(rootSymbol, context, out semanticModel, declarationOrder, cancellationToken);
			}

			semanticModel = null;
			return false;
		}

		protected virtual bool TryToInferGraphOrGraphExtensionSemanticModel(INamedTypeSymbol graphSymbol, PXContext context, 
																			GraphSemanticModelCreationOptions modelCreationOptions,
																			out ISemanticModel? graphSemanticModel,
																			CancellationToken cancellationToken = default)
		{
			var graphSimpleModel = PXGraphEventSemanticModel.InferModels(context, graphSymbol, modelCreationOptions, cancellationToken)
															.FirstOrDefault();

			if (graphSimpleModel == null)
			{
				graphSemanticModel = null;
				return false;
			}

			graphSemanticModel = new GraphSemanticModelForCodeMap(graphSimpleModel);
			return true;
		}

		protected virtual bool TryToInferDacOrDacExtensionSemanticModel(INamedTypeSymbol dacSymbol, PXContext context,
																		out ISemanticModel? dacSemanticModel,
																		int? declarationOrder,
																		CancellationToken cancellationToken = default)
		{
			dacSemanticModel = DacSemanticModel.InferModel(context, dacSymbol, declarationOrder, cancellationToken);
			return dacSemanticModel != null;
		}
	}
}
