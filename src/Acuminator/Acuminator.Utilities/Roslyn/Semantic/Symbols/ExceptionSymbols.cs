using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols
    {
	    private readonly Compilation _compilation;

        internal ExceptionSymbols(Compilation compilation)
        {
	        _compilation = compilation;
        }

	    public INamedTypeSymbol PXException => _compilation.GetTypeByMetadataName(typeof(PXException).FullName);
	    public INamedTypeSymbol PXBaseRedirectException => _compilation.GetTypeByMetadataName(typeof(PXBaseRedirectException).FullName);
	    public INamedTypeSymbol PXSetupNotEnteredException => _compilation.GetTypeByMetadataName(typeof(PXSetupNotEnteredException).FullName);
    }
}
