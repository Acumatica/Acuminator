using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols : SymbolsSetBase
    {
        internal ExceptionSymbols(Compilation compilation) : base(compilation)
        { }

	    public INamedTypeSymbol PXException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXException);
	    public INamedTypeSymbol PXBaseRedirectException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXBaseRedirectException);
	    public INamedTypeSymbol PXSetupNotEnteredException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXSetupNotEnteredException);

		public INamedTypeSymbol PXRowPersistedException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXRowPersistedException);

		public INamedTypeSymbol PXLockViolationException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXLockViolationException);
	}
}
