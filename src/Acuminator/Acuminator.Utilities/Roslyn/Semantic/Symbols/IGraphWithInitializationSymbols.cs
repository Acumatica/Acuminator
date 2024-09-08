using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols;

public class IGraphWithInitializationSymbols : SymbolsSetForTypeBase
{
	public IMethodSymbol? Initialize { get; }

	internal IGraphWithInitializationSymbols(Compilation compilation) : base(compilation, TypeFullNames.IGraphWithInitialization)
	{
		Initialize = Type.GetMethods(DelegateNames.Initialize)
						 .FirstOrDefault(m => m.IsValidInitializeMethod());
	}
}
