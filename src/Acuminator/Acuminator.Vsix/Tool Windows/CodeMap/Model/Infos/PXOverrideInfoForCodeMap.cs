using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;




namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A simple information for code map about the px override method in graph. 
	/// Do not store extra info about a method being overriden or info about other overrides 
	/// </summary>
	public class PXOverrideInfoForCodeMap : SymbolItem<IMethodSymbol>
	{
		public PXOverrideInfoForCodeMap(IMethodSymbol symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}
	}
}
