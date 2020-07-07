using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXSystemActionSymbols : SymbolsSetBase
	{
		internal PXSystemActionSymbols(Compilation compilation) : base(compilation)
		{
		}

		public INamedTypeSymbol PXSave => Compilation.GetTypeByMetadataName(DelegateNames.PXSave);
		public INamedTypeSymbol PXCancel => Compilation.GetTypeByMetadataName(DelegateNames.PXCancel);
		public INamedTypeSymbol PXInsert => Compilation.GetTypeByMetadataName(DelegateNames.PXInsert);
		public INamedTypeSymbol PXDelete => Compilation.GetTypeByMetadataName(DelegateNames.PXDelete);
		public INamedTypeSymbol PXCopyPasteAction => Compilation.GetTypeByMetadataName(DelegateNames.PXCopyPasteAction);
		public INamedTypeSymbol PXFirst => Compilation.GetTypeByMetadataName(DelegateNames.PXFirst);
		public INamedTypeSymbol PXPrevious => Compilation.GetTypeByMetadataName(DelegateNames.PXPrevious);
		public INamedTypeSymbol PXNext => Compilation.GetTypeByMetadataName(DelegateNames.PXNext);
		public INamedTypeSymbol PXLast => Compilation.GetTypeByMetadataName(DelegateNames.PXLast);
		public INamedTypeSymbol PXChangeID => Compilation.GetTypeByMetadataName(DelegateNames.PXChangeID);
	}
}
