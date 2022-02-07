#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphSemanticModelForCodeMap : ISemanticModel
	{
		public PXGraphEventSemanticModel GraphModel { get; }

		public ImmutableArray<PXOverrideInfoForCodeMap> PXOverrides { get; }

		public ImmutableArray<InstanceConstructorInfoForCodeMap> InstanceConstructors { get; }

		public INamedTypeSymbol Symbol => GraphModel.Symbol;

		public GraphSemanticModelForCodeMap(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
		{
			graphEventSemanticModel.ThrowOnNull(nameof(graphEventSemanticModel));
			context.ThrowOnNull(nameof(context));

			GraphModel = graphEventSemanticModel;
			PXOverrides = GetPXOverrides(graphEventSemanticModel.Symbol, context).ToImmutableArray();
			InstanceConstructors = GetInstanceConstructors(graphEventSemanticModel.Symbol, context).ToImmutableArray();
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

		protected virtual IEnumerable<InstanceConstructorInfoForCodeMap> GetInstanceConstructors(INamedTypeSymbol graphOrExtension, PXContext context)
		{
			if (graphOrExtension.InstanceConstructors.IsDefaultOrEmpty)
				yield break;

			var declaredConstructors = graphOrExtension.InstanceConstructors
													   .Where(constructor => constructor.ContainingType.Equals(graphOrExtension));
			int declarationOrder = 0;

			foreach (IMethodSymbol constructor in declaredConstructors)
			{
				yield return new InstanceConstructorInfoForCodeMap(constructor, declarationOrder);
				declarationOrder++;
			}
		}
	}
}
