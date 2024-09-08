#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphSemanticModelForCodeMap : ISemanticModel
	{
		public PXGraphEventSemanticModel GraphModel { get; }

		public ImmutableArray<InstanceConstructorInfo> InstanceConstructors { get; }

		public ImmutableArray<BaseMemberOverrideInfo> BaseMemberOverrides { get; }

		public INamedTypeSymbol Symbol => GraphModel.Symbol;

		public PXContext PXContext => GraphModel.PXContext;

		public GraphSemanticModelForCodeMap(PXGraphEventSemanticModel graphEventSemanticModel)
		{
			GraphModel 			 = graphEventSemanticModel.CheckIfNull();
			InstanceConstructors = GetInstanceConstructors(graphEventSemanticModel.Symbol).ToImmutableArray();
			BaseMemberOverrides  = GetBaseMemberOverrides(graphEventSemanticModel.Symbol).ToImmutableArray();
		}

		protected virtual IEnumerable<InstanceConstructorInfo> GetInstanceConstructors(INamedTypeSymbol graphOrExtension)
		{
			if (graphOrExtension.InstanceConstructors.IsDefaultOrEmpty)
				yield break;

			var declaredConstructors = from constructor in graphOrExtension.InstanceConstructors
									   where !constructor.IsImplicitlyDeclared && constructor.IsDeclaredInType(graphOrExtension)
									   select constructor;
			int declarationOrder = 0;

			foreach (IMethodSymbol constructor in declaredConstructors)
			{
				yield return new InstanceConstructorInfo(constructor, declarationOrder);
				declarationOrder++;
			}
		}

		protected virtual IEnumerable<BaseMemberOverrideInfo> GetBaseMemberOverrides(INamedTypeSymbol graphOrExtension)
		{
			var baseMemberOverrides = from member in graphOrExtension.GetMembers()
									  where !member.IsImplicitlyDeclared && member.IsOverride && 
											 member.CanBeReferencedByName && member.IsDeclaredInType(graphOrExtension)
									  select member;
			int declarationOrder = 0;

			foreach (ISymbol baseMemberOverride in baseMemberOverrides)
			{
				yield return new BaseMemberOverrideInfo(baseMemberOverride, GraphModel.BaseGraphModel, declarationOrder);
				declarationOrder++;
			}
		}
	}
}
