using Microsoft.CodeAnalysis;
using PX.Data;

namespace PX.Analyzers
{
	public class PXContext
	{
		internal PXContext(Compilation compilation)
		{
			Compilation = compilation;
		}

		public Compilation Compilation { get; }

		public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);
        public INamedTypeSymbol PXGraphExtensionType => Compilation.GetTypeByMetadataName(typeof(PXGraphExtension).FullName);
        public INamedTypeSymbol PXViewType => Compilation.GetTypeByMetadataName(typeof(PXView).FullName);
        public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
        public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
		public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
		public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
        //public INamedTypeSymbol PXBaseListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXBaseListAttribute).FullName);
        public INamedTypeSymbol PXStringListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXStringListAttribute).FullName);
        public INamedTypeSymbol PXIntListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXIntListAttribute).FullName);
        public INamedTypeSymbol PXIntAttributeType => Compilation.GetTypeByMetadataName(typeof(PXIntAttribute).FullName);
        public INamedTypeSymbol PXShortAttributeType => Compilation.GetTypeByMetadataName(typeof(PXShortAttribute).FullName);
        public INamedTypeSymbol PXStringAttributeType => Compilation.GetTypeByMetadataName(typeof(PXStringAttribute).FullName);
        public INamedTypeSymbol PXByteAttributeType => Compilation.GetTypeByMetadataName(typeof(PXByteAttribute).FullName);

        public INamedTypeSymbol PXDBIntAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBIntAttribute).FullName);
        public INamedTypeSymbol PXDBShortAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBShortAttribute).FullName);
        public INamedTypeSymbol PXDBStringAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBStringAttribute).FullName);
        public INamedTypeSymbol PXDBByteAttributeType => Compilation.GetTypeByMetadataName(typeof(PXDBByteAttribute).FullName);

    }
}
