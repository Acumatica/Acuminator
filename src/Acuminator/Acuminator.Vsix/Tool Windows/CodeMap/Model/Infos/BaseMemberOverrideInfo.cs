#nullable enable

using System;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public enum MemberOverrideKind : byte
	{
		None,
		NormalMethodOverride,
		NormalPropertyOverride,
		NormalEventOverride,
		PersistMethodOverride,
		ConfigureMethodOverride,
		InitializeMethodOverride
	}

	/// <summary>
	/// A simple information for code map about overrides of base members. 
	/// Do not store extra info about overidden members
	/// </summary>
	public class BaseMemberOverrideInfo : SymbolItem<ISymbol>
	{
		public MemberOverrideKind OverrideKind { get; }

		public BaseMemberOverrideInfo(ISymbol symbol, PXGraphSemanticModel graphSemanticModel, int declarationOrder) : 
								 base(symbol, declarationOrder)
		{
			OverrideKind = GetMethodOverrideKind(graphSemanticModel.CheckIfNull(), symbol);
		}

		private MemberOverrideKind GetMethodOverrideKind(PXGraphSemanticModel graphSemanticModel, ISymbol symbol)
		{
			switch (symbol)
			{
				case IPropertySymbol:
					return MemberOverrideKind.NormalPropertyOverride;

				case IEventSymbol:
					return MemberOverrideKind.NormalEventOverride;
				
				case IMethodSymbol methodSymbol:

					bool isConfigureMethod = IsConfigureOverride(methodSymbol, graphSemanticModel);
					if (isConfigureMethod)
						return MemberOverrideKind.ConfigureMethodOverride;

					bool isInitializeMethod = IsInitializeMethod(methodSymbol, graphSemanticModel);
					if (isInitializeMethod)
						return MemberOverrideKind.InitializeMethodOverride;

					bool isPersistMethod = IsPersistOverride();
					return isPersistMethod
						? MemberOverrideKind.PersistMethodOverride
						: MemberOverrideKind.NormalMethodOverride;

				default:
					return MemberOverrideKind.None;
			}
		}

		private static bool IsConfigureOverride(IMethodSymbol methodSymbol, PXGraphSemanticModel graphSemanticModel)
		{
			if (graphSemanticModel.ConfigureMethodOverride == null)
				return false;

			return graphSemanticModel.ConfigureMethodOverride
									 .ThisAndOverridenItems()
									 .Any(configureMethodInfo => methodSymbol.Equals(configureMethodInfo.Symbol, SymbolEqualityComparer.Default));
		}

		private static bool IsInitializeMethod(IMethodSymbol methodSymbol, PXGraphSemanticModel graphSemanticModel)
		{
			if (graphSemanticModel.InitializeMethodInfo == null)
				return false;

			return graphSemanticModel.InitializeMethodInfo
									 .ThisAndOverridenItems()
									 .Any(initializeMethodInfo => methodSymbol.Equals(initializeMethodInfo.Symbol, SymbolEqualityComparer.Default));
		}

		private bool IsPersistOverride() =>
			Symbol is IMethodSymbol method && method.ReturnsVoid && method.DeclaredAccessibility == Accessibility.Public &&
			method.Parameters.IsDefaultOrEmpty && method.Name == DelegateNames.Persist;
	}
}
