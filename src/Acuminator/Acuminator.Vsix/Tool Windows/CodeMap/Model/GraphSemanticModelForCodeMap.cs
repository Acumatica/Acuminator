using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphSemanticModelForCodeMap
	{
		public PXGraphEventSemanticModel GraphModel { get; }

		public ImmutableArray<PXOverrideInfoForCodeMap> PXOverrides { get; }

		public GraphSemanticModelForCodeMap(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
		{
			graphEventSemanticModel.ThrowOnNull(nameof(graphEventSemanticModel));
			context.ThrowOnNull(nameof(context));

			GraphModel = graphEventSemanticModel;
			PXOverrides = GetPXOverrides(graphEventSemanticModel.Symbol, context).ToImmutableArray();
		}

		protected virtual IEnumerable<PXOverrideInfoForCodeMap> GetPXOverrides(INamedTypeSymbol graphOrExtension, PXContext context)
		{
			var pxOverrideAttribute = context.AttributeTypes.PXOverrideAttribute;

			if (pxOverrideAttribute == null)
				yield break;

			var declaredMethods = graphOrExtension.GetMembers()
												  .OfType<IMethodSymbol>()
												  .Where(method => method.ContainingType.Equals(graphOrExtension));
			int declarationOrder = 0;

			foreach (IMethodSymbol method in declaredMethods)
			{
				var attributes = method.GetAttributes();

				if (!attributes.IsEmpty && attributes.Any(a => a.AttributeClass.Equals(pxOverrideAttribute)))
				{
					yield return new PXOverrideInfoForCodeMap(method, declarationOrder);
					declarationOrder++;
				}
			}
		}
	}
}
