using System;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class AttributeSymbols : SymbolsSetBase
	{
		public INamedTypeSymbol ObsoleteAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.ObsoleteAttribute);

		public INamedTypeSymbol PXInternalUseOnlyAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXInternalUseOnlyAttribute);

		public INamedTypeSymbol PXImportAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXImportAttribute);
		public INamedTypeSymbol PXHiddenAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXHiddenAttribute);
		public INamedTypeSymbol PXCacheNameAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXCacheNameAttribute);
		public INamedTypeSymbol PXPrimaryGraphAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXPrimaryGraphAttribute);
		public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXCopyPasteHiddenViewAttribute);
		public INamedTypeSymbol PXOverrideAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXOverrideAttribute);

		public INamedTypeSymbol PXEventSubscriberAttribute  => Compilation.GetTypeByMetadataName(TypeFullNames.PXEventSubscriberAttribute);
		public INamedTypeSymbol PXAttributeFamily           => Compilation.GetTypeByMetadataName(TypeFullNames.PXAttributeFamilyAttribute);
		public INamedTypeSymbol PXAggregateAttribute        => Compilation.GetTypeByMetadataName(TypeFullNames.PXAggregateAttribute);
		public INamedTypeSymbol PXDynamicAggregateAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDynamicAggregateAttribute);
		public INamedTypeSymbol PXDefaultAttribute          => Compilation.GetTypeByMetadataName(TypeFullNames.PXDefaultAttribute);
		public INamedTypeSymbol PXUnboundDefaultAttribute   => Compilation.GetTypeByMetadataName(TypeFullNames.PXUnboundDefaultAttribute);
        public INamedTypeSymbol PXButtonAttribute           => Compilation.GetTypeByMetadataName(TypeFullNames.PXButtonAttribute);
        public INamedTypeSymbol PXParentAttribute           => Compilation.GetTypeByMetadataName(TypeFullNames.PXParentAttribute);
        public INamedTypeSymbol PXDBDefaultAttribute        => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBDefaultAttribute);
        public INamedTypeSymbol PXForeignReferenceAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXForeignReferenceAttribute);
        public INamedTypeSymbol PXDimensionSelectorAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDimensionSelectorAttribute);

		private readonly Lazy<PXUIFieldAttributeSymbols> _pxUiFieldAttribute;
		public PXUIFieldAttributeSymbols PXUIFieldAttribute => _pxUiFieldAttribute.Value;

		private readonly Lazy<PXSelectorAttributeSymbols> _pxSelectorAttribute;
		public PXSelectorAttributeSymbols PXSelectorAttribute => _pxSelectorAttribute.Value;

		private readonly Lazy<PXStringListAttributeSymbols> _pxStringListAttribute;
		public PXStringListAttributeSymbols PXStringListAttribute => _pxStringListAttribute.Value;

		private readonly Lazy<PXIntListAttributeSymbols> _pxIntListAttribute;
		public PXIntListAttributeSymbols PXIntListAttribute => _pxIntListAttribute.Value;

		private readonly Lazy<AutoNumberAttributeSymbols> _autoNumberAttribute;
		public AutoNumberAttributeSymbols AutoNumberAttribute => _autoNumberAttribute.Value;

		internal AttributeSymbols(Compilation compilation) : base(compilation)
		{
			_pxUiFieldAttribute = new Lazy<PXUIFieldAttributeSymbols>(() => new PXUIFieldAttributeSymbols(compilation));
			_pxSelectorAttribute = new Lazy<PXSelectorAttributeSymbols>(() => new PXSelectorAttributeSymbols(compilation));
			_pxStringListAttribute = new Lazy<PXStringListAttributeSymbols>(() => new PXStringListAttributeSymbols(compilation));
			_pxIntListAttribute = new Lazy<PXIntListAttributeSymbols>(() => new PXIntListAttributeSymbols(compilation));
			_autoNumberAttribute = new Lazy<AutoNumberAttributeSymbols>(() => new AutoNumberAttributeSymbols(compilation));
		}
	}
}
