using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;




namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A simple information for code map about graph instance constructor. 
	/// Do not store extra info about base constructors
	/// </summary>
	public class InstanceConstructorInfoForCodeMap : SymbolItem<IMethodSymbol>
	{
		public InstanceConstructorInfoForCodeMap(IMethodSymbol symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}
	}
}
