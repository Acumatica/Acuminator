using System;
using System.Collections.Immutable;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn
{
	public static class BqlModifyingMethods
	{
		public static readonly ImmutableHashSet<string> PXSelectbaseBqlModifiers = ImmutableHashSet.Create
		(
			DelegateNames.WhereAnd,
			DelegateNames.WhereNew,
			DelegateNames.WhereOr,
			DelegateNames.Join
		);

		public static readonly ImmutableHashSet<string> PXViewBqlModifiers = ImmutableHashSet.Create
		(
			DelegateNames.WhereAnd,
			DelegateNames.WhereNew,
			DelegateNames.WhereOr,
			DelegateNames.Join,
			DelegateNames.JoinNew
		);

		public static readonly ImmutableHashSet<string> BqlCommandInstanceBqlModifiers = ImmutableHashSet.Create
		(
			DelegateNames.WhereAnd,
			DelegateNames.WhereNew,
			DelegateNames.WhereOr,
			DelegateNames.AggregateNew,
			DelegateNames.OrderByNew
		);

		public static readonly ImmutableHashSet<string> BqlCommandStaticBqlModifiers = ImmutableHashSet.Create
		(
			DelegateNames.Compose,
			DelegateNames.CreateInstance,
			DelegateNames.AddJoinConditions,
			DelegateNames.AppendJoin,
			DelegateNames.NewJoin
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
