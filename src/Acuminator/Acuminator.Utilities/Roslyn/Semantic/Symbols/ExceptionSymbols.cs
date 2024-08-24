#nullable enable

using System;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;
using System.Linq;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class ExceptionSymbols : SymbolsSetBase
    {
		public INamedTypeSymbol Exception { get; }

		public INamedTypeSymbol PXException { get; }

		public INamedTypeSymbol? PXExceptionInfo { get; }

		public IPropertySymbol? PXException_Message { get; }

		public IPropertySymbol? PXExceptionInfo_MessageFormat { get; }

		public IPropertySymbol? PXExceptionInfo_MessageArguments { get; }


		public INamedTypeSymbol PXBaseRedirectException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXBaseRedirectException)!;
	    public INamedTypeSymbol PXSetupNotEnteredException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXSetupNotEnteredException)!;

		public INamedTypeSymbol PXRowPersistedException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXRowPersistedException)!;

		public INamedTypeSymbol PXLockViolationException => Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXLockViolationException)!;


		public INamedTypeSymbol ArgumentException => Compilation.GetTypeByMetadataName($"{nameof(System)}.{nameof(System.ArgumentException)}")!;
		public INamedTypeSymbol NotSupportedException => Compilation.GetTypeByMetadataName($"{nameof(System)}.{nameof(System.NotSupportedException)}")!;
		public INamedTypeSymbol NotImplementedException => Compilation.GetTypeByMetadataName($"{nameof(System)}.{nameof(System.NotImplementedException)}")!;

		internal ExceptionSymbols(Compilation compilation) : base(compilation)
		{
			Exception = Compilation.GetTypeByMetadataName(typeof(Exception).FullName)!;
			PXException = Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXException)!;
			PXExceptionInfo = Compilation.GetTypeByMetadataName(TypeFullNames.Exceptions.PXExceptionInfo);

			PXException_Message = GetProperty(PXException, PropertyNames.Exception.Message);
			PXExceptionInfo_MessageFormat = GetProperty(PXExceptionInfo, PropertyNames.Exception.MessageFormat);
			PXExceptionInfo_MessageArguments = GetProperty(PXExceptionInfo, PropertyNames.Exception.MessageArguments);
		}

		private IPropertySymbol? GetProperty(INamedTypeSymbol? type, string propertyName) =>
			type?.GetMembers(propertyName)
				 .OfType<IPropertySymbol>()
				 .FirstOrDefault();
	}
}
