using System;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class AttributeSymbols
	{
		private readonly Compilation _compilation;

		internal AttributeSymbols(Compilation compilation)
		{
			_compilation = compilation;

			_pxUiFieldAttribute = new Lazy<PXUIFieldAttributeSymbols>(() => new PXUIFieldAttributeSymbols(compilation));
			_pxSelectorAttribute = new Lazy<PXSelectorAttributeSymbols>(() => new PXSelectorAttributeSymbols(compilation));
			_pxStringListAttribute = new Lazy<PXStringListAttributeSymbols>(() => new PXStringListAttributeSymbols(compilation));
			_pxIntListAttribute = new Lazy<PXIntListAttributeSymbols>(() => new PXIntListAttributeSymbols(compilation));
		}

		public INamedTypeSymbol PXImportAttribute => _compilation.GetTypeByMetadataName(Types.PXImportAttribute);
		public INamedTypeSymbol PXHiddenAttribute => _compilation.GetTypeByMetadataName(Types.PXHiddenAttribute);
		public INamedTypeSymbol PXCacheNameAttribute => _compilation.GetTypeByMetadataName(Types.PXCacheNameAttribute);
		public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => _compilation.GetTypeByMetadataName(Types.PXCopyPasteHiddenViewAttribute);
		public INamedTypeSymbol PXOverrideAttribute => _compilation.GetTypeByMetadataName(Types.PXOverrideAttribute);

		public INamedTypeSymbol PXEventSubscriberAttribute => _compilation.GetTypeByMetadataName(Types.PXEventSubscriberAttribute);
		public INamedTypeSymbol PXAttributeFamily => _compilation.GetTypeByMetadataName(Types.PXAttributeFamilyAttribute);
		public INamedTypeSymbol PXAggregateAttribute => _compilation.GetTypeByMetadataName(Types.PXAggregateAttribute);
		public INamedTypeSymbol PXDynamicAggregateAttribute => _compilation.GetTypeByMetadataName(Types.PXDynamicAggregateAttribute);
		public INamedTypeSymbol PXDefaultAttribute => _compilation.GetTypeByMetadataName(Types.PXDefaultAttribute);
		public INamedTypeSymbol PXUnboundDefaultAttribute => _compilation.GetTypeByMetadataName(Types.PXUnboundDefaultAttribute);
        public INamedTypeSymbol PXButtonAttribute => _compilation.GetTypeByMetadataName(Types.PXButtonAttribute);

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
