using System;
using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn
{
	internal class DummyDac : IBqlTable { }

	public static class BqlModifyingMethods
	{
		public static readonly ImmutableHashSet<string> PXSelectbaseBqlModifiers = ImmutableHashSet.Create
		(
			nameof(PXSelectBase<DummyDac>.WhereAnd),
			nameof(PXSelectBase<DummyDac>.WhereNew),
			nameof(PXSelectBase<DummyDac>.WhereOr),
			nameof(PXSelectBase<DummyDac>.Join)
		);

		public static readonly ImmutableHashSet<string> PXViewBqlModifiers = ImmutableHashSet.Create
		(
			nameof(PXView.WhereAnd),
			nameof(PXView.WhereNew),
			nameof(PXView.WhereOr),
			nameof(PXView.Join),
			nameof(PXView.JoinNew)
		);

		public static readonly ImmutableHashSet<string> BqlCommandInstanceBqlModifiers = ImmutableHashSet.Create
		(
			nameof(BqlCommand.WhereAnd),
			nameof(BqlCommand.WhereNew),
			nameof(BqlCommand.WhereOr),
			nameof(BqlCommand.AggregateNew),
			nameof(BqlCommand.OrderByNew)
		);

		public static readonly ImmutableHashSet<string> BqlCommandStaticBqlModifiers = ImmutableHashSet.Create
		(
			nameof(BqlCommand.Compose),
			nameof(BqlCommand.CreateInstance),
			nameof(BqlCommand.AddJoinConditions),
			nameof(BqlCommand.AppendJoin),
			nameof(BqlCommand.NewJoin)
		);


		public static bool IsBqlModifyingInstanceMethod(IMethodSymbol methodSymbol, PXContext context)
		{
			methodSymbol.ThrowOnNull(nameof(methodSymbol));

			if (methodSymbol.IsStatic)
			{
				throw new ArgumentException("Method argument must be an instance method", nameof(methodSymbol));
			}

			if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
				return false;

			var containingType = methodSymbol.ContainingType;

			if (containingType == null)
				return false;
			else if (containingType.Equals(context.BQL.BqlCommand))
				return BqlCommandInstanceBqlModifiers.Contains(methodSymbol.Name);
			else if (containingType.Equals(context.BQL.PXSelectBaseGenericType) ||
					 containingType.OriginalDefinition.Equals(context.BQL.PXSelectBaseGenericType))
			{
				return PXSelectbaseBqlModifiers.Contains(methodSymbol.Name);
			}
			else if (containingType.Equals(context.PXView.Type))
				return PXViewBqlModifiers.Contains(methodSymbol.Name);
			else
				return false;
		}
	}
}
