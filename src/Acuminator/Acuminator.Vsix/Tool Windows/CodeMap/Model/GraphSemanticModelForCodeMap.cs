﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphSemanticModelForCodeMap : ISemanticModel
	{
		public PXGraphEventSemanticModel GraphModel { get; }

		public ImmutableArray<InstanceConstructorInfo> InstanceConstructors { get; }

		public ImmutableArray<BaseMemberOverrideInfo> BaseMemberOverrides { get; }

		public INamedTypeSymbol Symbol => GraphModel.Symbol;

		public GraphSemanticModelForCodeMap(PXGraphEventSemanticModel graphEventSemanticModel, PXContext context)
		{
			graphEventSemanticModel.ThrowOnNull(nameof(graphEventSemanticModel));
			context.ThrowOnNull(nameof(context));

			GraphModel = graphEventSemanticModel;
			InstanceConstructors = GetInstanceConstructors(graphEventSemanticModel.Symbol, context).ToImmutableArray();
			BaseMemberOverrides = GetBaseMemberOverrides(graphEventSemanticModel.Symbol, context).ToImmutableArray();
		}

		protected virtual IEnumerable<InstanceConstructorInfo> GetInstanceConstructors(INamedTypeSymbol graphOrExtension, PXContext context)
		{
			if (graphOrExtension.InstanceConstructors.IsDefaultOrEmpty)
				yield break;

			var declaredConstructors = from constructor in graphOrExtension.InstanceConstructors
									   where !constructor.IsImplicitlyDeclared && 
											  constructor.ContainingType.Equals(graphOrExtension)
									   select constructor;
			int declarationOrder = 0;

			foreach (IMethodSymbol constructor in declaredConstructors)
			{
				yield return new InstanceConstructorInfo(constructor, declarationOrder);
				declarationOrder++;
			}
		}

		protected virtual IEnumerable<BaseMemberOverrideInfo> GetBaseMemberOverrides(INamedTypeSymbol graphOrExtension, PXContext context)
		{
			var baseMemberOverrides = from member in graphOrExtension.GetMembers()
									  where !member.IsImplicitlyDeclared && member.IsOverride && 
											 member.CanBeReferencedByName &&
											 member.ContainingType.Equals(graphOrExtension)
									  select member;
			int declarationOrder = 0;

			foreach (ISymbol baseMemberOverride in baseMemberOverrides)
			{
				yield return new BaseMemberOverrideInfo(baseMemberOverride, declarationOrder);
				declarationOrder++;
			}
		}
	}
}
