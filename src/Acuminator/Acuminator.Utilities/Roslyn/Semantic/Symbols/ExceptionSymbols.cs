using System;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols : SymbolsSetBase
    {
        internal ExceptionSymbols(Compilation compilation) : base(compilation)
        {
			Exception = Compilation.GetTypeByMetadataName(typeof(Exception).FullName);
			PXException = Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXException);
		}

		public INamedTypeSymbol Exception { get; }

		public INamedTypeSymbol PXException { get; }

	    public INamedTypeSymbol PXBaseRedirectException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXBaseRedirectException);
	    public INamedTypeSymbol PXSetupNotEnteredException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXSetupNotEnteredException);

		public INamedTypeSymbol PXRowPersistedException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXRowPersistedException);

		public INamedTypeSymbol PXLockViolationException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXLockViolationException);


		public INamedTypeSymbol ArgumentException => Compilation.GetTypeByMetadataName($"{nameof(System)}.{nameof(System.ArgumentException)}");
		public INamedTypeSymbol NotSupportedException => Compilation.GetTypeByMetadataName($"{nameof(System)}.{nameof(System.NotSupportedException)}");
		public INamedTypeSymbol NotImplementedException => Compilation.GetTypeByMetadataName($"{nameof(System)}.{nameof(System.NotImplementedException)}");
	}
}
