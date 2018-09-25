using System;
using Microsoft.CodeAnalysis;
using PX.Data;

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

		public INamedTypeSymbol PXImportAttribute => _compilation.GetTypeByMetadataName(typeof(PXImportAttribute).FullName);
		public INamedTypeSymbol PXHiddenAttribute => _compilation.GetTypeByMetadataName(typeof(PXHiddenAttribute).FullName);
		public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => _compilation.GetTypeByMetadataName(typeof(PXCopyPasteHiddenViewAttribute).FullName);
		public INamedTypeSymbol PXOverrideAttribute => _compilation.GetTypeByMetadataName(typeof(PXOverrideAttribute).FullName);

		public INamedTypeSymbol PXEventSubscriberAttribute => _compilation.GetTypeByMetadataName(typeof(PXEventSubscriberAttribute).FullName);
		public INamedTypeSymbol PXAttributeFamily => _compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);
		public INamedTypeSymbol PXAggregateAttribute => _compilation.GetTypeByMetadataName(typeof(PXAggregateAttribute).FullName);
		public INamedTypeSymbol PXDynamicAggregateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDynamicAggregateAttribute).FullName);
		public INamedTypeSymbol PXDefaultAttribute => _compilation.GetTypeByMetadataName(typeof(PXDefaultAttribute).FullName);
		public INamedTypeSymbol PXUnboundDefaultAttribute => _compilation.GetTypeByMetadataName(typeof(PXUnboundDefaultAttribute).FullName);

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
