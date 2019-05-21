using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols
    {
	    private readonly Compilation _compilation;

        internal ExceptionSymbols(Compilation compilation)
        {
	        _compilation = compilation;
        }

	    public INamedTypeSymbol PXException => _compilation.GetTypeByMetadataName(Types.PXException);
	    public INamedTypeSymbol PXBaseRedirectException => _compilation.GetTypeByMetadataName(Types.PXBaseRedirectException);
	    public INamedTypeSymbol PXSetupNotEnteredException => _compilation.GetTypeByMetadataName(Types.PXSetupNotEnteredException);
    }
}
