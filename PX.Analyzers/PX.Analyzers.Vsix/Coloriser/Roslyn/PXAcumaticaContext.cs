using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using PX.Data;

namespace PX.Analyzers.Coloriser
{
	public class PXAcumaticaContext
	{
        private readonly Compilation compilation;
		
		public INamedTypeSymbol PXGraphType => compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);      
		public INamedTypeSymbol PXViewType => compilation.GetTypeByMetadataName(typeof(PXView).FullName);
        public INamedTypeSymbol PXSelectBaseType => compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);      	
		public INamedTypeSymbol IBqlTableType => compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);

        #region BQL Types
        public INamedTypeSymbol PXSelectBase => compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
        public INamedTypeSymbol PXSelect => compilation.GetTypeByMetadataName(typeof(PXSelect<>).FullName);
        public INamedTypeSymbol PXSelectJoin1 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,>).FullName);
        public INamedTypeSymbol PXSelectJoin2 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,,>).FullName);
        public INamedTypeSymbol PXSelectJoin3 => compilation.GetTypeByMetadataName(typeof(PXSelectJoin<,,,>).FullName);
        public INamedTypeSymbol OrderBy => compilation.GetTypeByMetadataName(typeof(OrderBy<>).FullName);
        public INamedTypeSymbol Asc => compilation.GetTypeByMetadataName(typeof(Asc<>).FullName);
        public INamedTypeSymbol AscWithContinuation => compilation.GetTypeByMetadataName(typeof(Asc<,>).FullName);
        public INamedTypeSymbol Desc => compilation.GetTypeByMetadataName(typeof(Desc<>).FullName);
        public INamedTypeSymbol DescWithContinuation => compilation.GetTypeByMetadataName(typeof(Desc<,>).FullName);
        #endregion

        public PXAcumaticaContext(Compilation aCompilation)
        {
            aCompilation.ThrowOnNull(nameof(aCompilation));

            compilation = aCompilation;
        }
    }
}
