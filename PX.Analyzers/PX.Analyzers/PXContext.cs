using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using PX.Data;

namespace PX.Analyzers
{
	public class PXContext
	{
		internal PXContext(Compilation compilation)
		{
			Compilation = compilation;
			BQL = new BQLSymbols(Compilation);
		}

		public Compilation Compilation { get; }

		public BQLSymbols BQL { get; }

		public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);
        public INamedTypeSymbol PXProcessingBaseType => Compilation.GetTypeByMetadataName(typeof(PXProcessingBase<>).FullName);
        public INamedTypeSymbol PXGraphExtensionType => Compilation.GetTypeByMetadataName(typeof(PXGraphExtension).FullName);
		public INamedTypeSymbol PXCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXCacheExtension).FullName);
		public INamedTypeSymbol PXViewType => Compilation.GetTypeByMetadataName(typeof(PXView).FullName);
        public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
        public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
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


		#region BQL Types
		/// <summary>
		/// BQL Symbols are stored in separate file.
		/// </summary>
		public class BQLSymbols
		{
			private readonly Compilation compilation;

			public BQLSymbols(Compilation aCompilation)
			{
				compilation = aCompilation;
			}

			public INamedTypeSymbol PXSelectBase => compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);

			public INamedTypeSymbol PXSelect => compilation.GetTypeByMetadataName(typeof(PXSelect<>).FullName);

			public INamedTypeSymbol PXSelectJoin1 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,>).FullName);

			public INamedTypeSymbol PXSelectJoin2 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,,>).FullName);
			public INamedTypeSymbol PXSelectJoin3 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,,,>).FullName);
		}
		#endregion
	}
}
