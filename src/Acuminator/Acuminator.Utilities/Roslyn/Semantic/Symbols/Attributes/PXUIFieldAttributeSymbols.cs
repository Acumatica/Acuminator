using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXUIFieldAttributeSymbols : SymbolsSetForTypeBase
	{
		public ImmutableArray<IMethodSymbol> SetVisible { get; }
		public ImmutableArray<IMethodSymbol> SetVisibility { get; }
		public ImmutableArray<IMethodSymbol> SetEnabled { get; }
		public ImmutableArray<IMethodSymbol> SetRequired { get; }
		public ImmutableArray<IMethodSymbol> SetReadOnly { get; }
		public ImmutableArray<IMethodSymbol> SetDisplayName { get; }
		public ImmutableArray<IMethodSymbol> SetNeutralDisplayName { get; }

		internal PXUIFieldAttributeSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXUIFieldAttribute)
		{
			SetVisible 			  = Type.GetMethods(DelegateNames.SetVisible).ToImmutableArray();
			SetVisibility 		  = Type.GetMethods(DelegateNames.SetVisibility).ToImmutableArray();
			SetEnabled 			  = Type.GetMethods(DelegateNames.SetEnabled).ToImmutableArray();
			SetRequired 		  = Type.GetMethods(DelegateNames.SetRequired).ToImmutableArray();
			SetReadOnly 		  = Type.GetMethods(DelegateNames.SetReadOnly).ToImmutableArray();
			SetDisplayName 		  = Type.GetMethods(DelegateNames.SetDisplayName).ToImmutableArray();
			SetNeutralDisplayName = Type.GetMethods(DelegateNames.SetNeutralDisplayName).ToImmutableArray();
		}
	}
}
