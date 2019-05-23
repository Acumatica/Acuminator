using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;


namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXSystemActionSymbols
	{
		private readonly Compilation _compilation;

		internal PXSystemActionSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol PXSave => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXSave);
		public INamedTypeSymbol PXCancel => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXCancel);
		public INamedTypeSymbol PXInsert => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXInsert);
		public INamedTypeSymbol PXDelete => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXDelete);
		public INamedTypeSymbol PXCopyPasteAction => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXCopyPasteAction);
		public INamedTypeSymbol PXFirst => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXFirst);
		public INamedTypeSymbol PXPrevious => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXPrevious);
		public INamedTypeSymbol PXNext => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXNext);
		public INamedTypeSymbol PXLast => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXLast);
		public INamedTypeSymbol PXChangeID => _compilation.GetTypeByMetadataName(Types.PXSystemActionSymbols.PXChangeID);
	}
}
