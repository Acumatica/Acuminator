using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols : SymbolsSetBase
    {
        internal ExceptionSymbols(Compilation compilation) : base(compilation)
        { }

	    public INamedTypeSymbol PXException => Compilation.GetTypeByMetadataName(TypeFullNames.PXException);
	    public INamedTypeSymbol PXBaseRedirectException => Compilation.GetTypeByMetadataName(TypeFullNames.PXBaseRedirectException);
	    public INamedTypeSymbol PXSetupNotEnteredException => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetupNotEnteredException);
    }
}
