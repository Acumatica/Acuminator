using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class PXSystemActionSymbols
	{
		private readonly Compilation _compilation;

		internal PXSystemActionSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol PXSave => _compilation.GetTypeByMetadataName(typeof(PXSave<>).FullName);
		public INamedTypeSymbol PXCancel => _compilation.GetTypeByMetadataName(typeof(PXCancel<>).FullName);
		public INamedTypeSymbol PXInsert => _compilation.GetTypeByMetadataName(typeof(PXInsert<>).FullName);
		public INamedTypeSymbol PXDelete => _compilation.GetTypeByMetadataName(typeof(PXDelete<>).FullName);
		public INamedTypeSymbol PXCopyPasteAction => _compilation.GetTypeByMetadataName(typeof(PXCopyPasteAction<>).FullName);
		public INamedTypeSymbol PXFirst => _compilation.GetTypeByMetadataName(typeof(PXFirst<>).FullName);
		public INamedTypeSymbol PXPrevious => _compilation.GetTypeByMetadataName(typeof(PXPrevious<>).FullName);
		public INamedTypeSymbol PXNext => _compilation.GetTypeByMetadataName(typeof(PXNext<>).FullName);
		public INamedTypeSymbol PXLast => _compilation.GetTypeByMetadataName(typeof(PXLast<>).FullName);
		public INamedTypeSymbol PXChangeID => _compilation.GetTypeByMetadataName(typeof(PXChangeID<,>).FullName);
	}
}
