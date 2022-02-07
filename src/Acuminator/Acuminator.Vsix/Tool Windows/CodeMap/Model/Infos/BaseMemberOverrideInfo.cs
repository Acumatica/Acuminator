#nullable enable

using System;
using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A simple information for code map about overrides of base members. 
	/// Do not store extra info about overidden members
	/// </summary>
	public class BaseMemberOverrideInfo : SymbolItem<ISymbol>
	{
		public BaseMemberOverrideInfo(ISymbol symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}
	}
}
