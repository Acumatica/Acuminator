using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols : SymbolsSetBase
    {
        internal ExceptionSymbols(Compilation compilation) : base(compilation)
        { }

	    public INamedTypeSymbol PXException => _compilation.GetTypeByMetadataName(TypeFullNames.PXException);
	    public INamedTypeSymbol PXBaseRedirectException => _compilation.GetTypeByMetadataName(TypeFullNames.PXBaseRedirectException);
	    public INamedTypeSymbol PXSetupNotEnteredException => _compilation.GetTypeByMetadataName(TypeFullNames.PXSetupNotEnteredException);
    }
}
