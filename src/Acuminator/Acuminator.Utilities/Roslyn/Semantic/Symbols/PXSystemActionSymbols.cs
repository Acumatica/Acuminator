using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXSystemActionSymbols
	{
		private readonly Compilation _compilation;

		internal PXSystemActionSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol PXSave => _compilation.GetTypeByMetadataName(DelegateNames.PXSave);
		public INamedTypeSymbol PXCancel => _compilation.GetTypeByMetadataName(DelegateNames.PXCancel);
		public INamedTypeSymbol PXInsert => _compilation.GetTypeByMetadataName(DelegateNames.PXInsert);
		public INamedTypeSymbol PXDelete => _compilation.GetTypeByMetadataName(DelegateNames.PXDelete);
		public INamedTypeSymbol PXCopyPasteAction => _compilation.GetTypeByMetadataName(DelegateNames.PXCopyPasteAction);
		public INamedTypeSymbol PXFirst => _compilation.GetTypeByMetadataName(DelegateNames.PXFirst);
		public INamedTypeSymbol PXPrevious => _compilation.GetTypeByMetadataName(DelegateNames.PXPrevious);
		public INamedTypeSymbol PXNext => _compilation.GetTypeByMetadataName(DelegateNames.PXNext);
		public INamedTypeSymbol PXLast => _compilation.GetTypeByMetadataName(DelegateNames.PXLast);
		public INamedTypeSymbol PXChangeID => _compilation.GetTypeByMetadataName(DelegateNames.PXChangeID);
	}
}
