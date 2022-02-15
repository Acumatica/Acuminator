#nullable enable

using System;
using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A simple information for code map about overrides of base members. 
	/// Do not store extra info about overidden members
	/// </summary>
	public class BaseMemberOverrideInfo : SymbolItem<ISymbol>
	{
		public bool IsPersistMethodOverride { get; }

		public BaseMemberOverrideInfo(ISymbol symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
			IsPersistMethodOverride = IsPersistOverride();
		}

		private bool IsPersistOverride() =>
			Symbol is IMethodSymbol method && method.ReturnsVoid && method.DeclaredAccessibility == Accessibility.Public &&
			method.Parameters.IsDefaultOrEmpty && method.Name == DelegateNames.Persist;
	}
}
