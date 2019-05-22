using System;
using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn
{
	public static class BqlModifyingMethods
	{
		public static readonly ImmutableHashSet<string> PXSelectbaseBqlModifiers = ImmutableHashSet.Create
		(
			Types.PXSelectBaseDelegates.WhereAnd,
			Types.PXSelectBaseDelegates.WhereNew,
			Types.PXSelectBaseDelegates.WhereOr,
			Types.PXSelectBaseDelegates.Join
		);

		public static readonly ImmutableHashSet<string> PXViewBqlModifiers = ImmutableHashSet.Create
		(
			Types.PXViewDelegates.WhereAnd,
			Types.PXViewDelegates.WhereNew,
			Types.PXViewDelegates.WhereOr,
			Types.PXViewDelegates.Join,
			Types.PXViewDelegates.JoinNew
		);

		public static readonly ImmutableHashSet<string> BqlCommandInstanceBqlModifiers = ImmutableHashSet.Create
		(
			Types.BqlCommandDelegates.WhereAnd,
			Types.BqlCommandDelegates.WhereNew,
			Types.BqlCommandDelegates.WhereOr,
			Types.BqlCommandDelegates.AggregateNew,
			Types.BqlCommandDelegates.OrderByNew
		);

		public static readonly ImmutableHashSet<string> BqlCommandStaticBqlModifiers = ImmutableHashSet.Create
		(
			Types.BqlCommandDelegates.Compose,
			Types.BqlCommandDelegates.CreateInstance,
			Types.BqlCommandDelegates.AddJoinConditions,
			Types.BqlCommandDelegates.AppendJoin,
			Types.BqlCommandDelegates.NewJoin
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
