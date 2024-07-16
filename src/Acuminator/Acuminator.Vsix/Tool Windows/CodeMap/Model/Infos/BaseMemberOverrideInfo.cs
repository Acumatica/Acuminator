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
		ConfigureMethodOverride
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
					bool isConfigureMethod = graphSemanticModel.ConfigureMethodOverrides
															   .Any(configureMethodInfo => methodSymbol.Equals(configureMethodInfo.Symbol));
					if (isConfigureMethod)
						return MemberOverrideKind.ConfigureMethodOverride;

					bool isPersistMethod = IsPersistOverride();
					return isPersistMethod
						? MemberOverrideKind.PersistMethodOverride
						: MemberOverrideKind.NormalMethodOverride;

				default:
					return MemberOverrideKind.None;
			}
		}

		private bool IsPersistOverride() =>
			Symbol is IMethodSymbol method && method.ReturnsVoid && method.DeclaredAccessibility == Accessibility.Public &&
			method.Parameters.IsDefaultOrEmpty && method.Name == DelegateNames.Persist;
	}
}
