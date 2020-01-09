using System;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class AttributeSymbols : SymbolsSetBase
	{
		internal AttributeSymbols(Compilation compilation) : base(compilation)
		{
			_pxUiFieldAttribute = new Lazy<PXUIFieldAttributeSymbols>(() => new PXUIFieldAttributeSymbols(compilation));
			_pxSelectorAttribute = new Lazy<PXSelectorAttributeSymbols>(() => new PXSelectorAttributeSymbols(compilation));
			_pxStringListAttribute = new Lazy<PXStringListAttributeSymbols>(() => new PXStringListAttributeSymbols(compilation));
			_pxIntListAttribute = new Lazy<PXIntListAttributeSymbols>(() => new PXIntListAttributeSymbols(compilation));
		}

		public INamedTypeSymbol ObsoleteAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.ObsoleteAttribute);

		public INamedTypeSymbol PXInternalUseOnlyAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXInternalUseOnlyAttribute);

		public INamedTypeSymbol PXImportAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXImportAttribute);
		public INamedTypeSymbol PXHiddenAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXHiddenAttribute);
		public INamedTypeSymbol PXCacheNameAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXCacheNameAttribute);
		public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXCopyPasteHiddenViewAttribute);
		public INamedTypeSymbol PXOverrideAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXOverrideAttribute);

		public INamedTypeSymbol PXEventSubscriberAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXEventSubscriberAttribute);
		public INamedTypeSymbol PXAttributeFamily => _compilation.GetTypeByMetadataName(TypeFullNames.PXAttributeFamilyAttribute);
		public INamedTypeSymbol PXAggregateAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXAggregateAttribute);
		public INamedTypeSymbol PXDynamicAggregateAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDynamicAggregateAttribute);
		public INamedTypeSymbol PXDefaultAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDefaultAttribute);
		public INamedTypeSymbol PXUnboundDefaultAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXUnboundDefaultAttribute);
        public INamedTypeSymbol PXButtonAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXButtonAttribute);

		private readonly Lazy<PXUIFieldAttributeSymbols> _pxUiFieldAttribute;
		public PXUIFieldAttributeSymbols PXUIFieldAttribute => _pxUiFieldAttribute.Value;

		private readonly Lazy<PXSelectorAttributeSymbols> _pxSelectorAttribute;
		public PXSelectorAttributeSymbols PXSelectorAttribute => _pxSelectorAttribute.Value;

		private readonly Lazy<PXStringListAttributeSymbols> _pxStringListAttribute;
		public PXStringListAttributeSymbols PXStringListAttribute => _pxStringListAttribute.Value;

		private readonly Lazy<PXIntListAttributeSymbols> _pxIntListAttribute;
		public PXIntListAttributeSymbols PXIntListAttribute => _pxIntListAttribute.Value;
	}
}
