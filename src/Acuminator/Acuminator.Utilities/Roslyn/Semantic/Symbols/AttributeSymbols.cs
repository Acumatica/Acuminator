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
		}

		public INamedTypeSymbol PXImportAttribute => _compilation.GetTypeByMetadataName(typeof(PXImportAttribute).FullName);
		public INamedTypeSymbol PXHiddenAttribute => _compilation.GetTypeByMetadataName(typeof(PXHiddenAttribute).FullName);
		public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => _compilation.GetTypeByMetadataName(typeof(PXCopyPasteHiddenViewAttribute).FullName);
		public INamedTypeSymbol PXOverrideAttribute => _compilation.GetTypeByMetadataName(typeof(PXOverrideAttribute).FullName);

		public INamedTypeSymbol PXStringListAttribute => _compilation.GetTypeByMetadataName(typeof(PXStringListAttribute).FullName);
		public INamedTypeSymbol PXIntListAttribute => _compilation.GetTypeByMetadataName(typeof(PXIntListAttribute).FullName);

		public INamedTypeSymbol PXEventSubscriberAttribute => _compilation.GetTypeByMetadataName(typeof(PXEventSubscriberAttribute).FullName);
		public INamedTypeSymbol PXAttributeFamily => _compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);
		public INamedTypeSymbol PXAggregateAttribute => _compilation.GetTypeByMetadataName(typeof(PXAggregateAttribute).FullName);
		public INamedTypeSymbol PXDynamicAggregateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDynamicAggregateAttribute).FullName);
		public INamedTypeSymbol PXDefaultAttribute => _compilation.GetTypeByMetadataName(typeof(PXDefaultAttribute).FullName);
		public INamedTypeSymbol PXUnboundDefaultAttribute => _compilation.GetTypeByMetadataName(typeof(PXUnboundDefaultAttribute).FullName);

		private readonly Lazy<PXUIFieldAttributeSymbols> _pxUiFieldAttribute;
		public PXUIFieldAttributeSymbols PXUIFieldAttribute => _pxUiFieldAttribute.Value;
	}
}
