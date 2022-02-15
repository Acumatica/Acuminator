#nullable enable

using System;
using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A simple information for code map about graph instance constructor. 
	/// Do not store extra info about base constructors
	/// </summary>
	public class InstanceConstructorInfo : SymbolItem<IMethodSymbol>
	{
		public InstanceConstructorInfo(IMethodSymbol symbol, int declarationOrder) : base(symbol, declarationOrder)
		{
		}
	}
}
