using System.Linq;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXSelectBaseSymbols : SymbolsSetForTypeBase
	{
		public IFieldSymbol View { get; }

		internal PXSelectBaseSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXSelectBase)
		{
			View = Type.GetMembers(DelegateNames.View)
				   .OfType<IFieldSymbol>()
				   .First();
		}
	}
}