﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXStringListAttributeSymbols : SymbolsSetForTypeBase
    {
		public ImmutableArray<IMethodSymbol> SetList { get; }
	    public ImmutableArray<IMethodSymbol> AppendList { get; }
	    public ImmutableArray<IMethodSymbol> SetLocalizable { get; }

        internal PXStringListAttributeSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXStringListAttribute)
        {
	        SetList        = Type.GetMethods(DelegateNames.SetList);
	        AppendList     = Type.GetMethods(DelegateNames.AppendList);
	        SetLocalizable = Type.GetMethods(DelegateNames.SetLocalizable);
        }
    }
}
