using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// BQL Symbols are stored in separate file.
	/// </summary>
	public class BQLSymbols
	{
		private readonly Compilation _compilation;

		internal BQLSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		#region CustomDelegates
		public INamedTypeSymbol CustomPredicate => _compilation.GetTypeByMetadataName(typeof(CustomPredicate).FullName);

		public INamedTypeSymbol AreSame => _compilation.GetTypeByMetadataName(typeof(AreSame<,>).FullName);

		public INamedTypeSymbol AreDistinct => _compilation.GetTypeByMetadataName(typeof(AreDistinct<,>).FullName);
		#endregion

		public INamedTypeSymbol Required => _compilation.GetTypeByMetadataName(typeof(Required<>).FullName);

		public INamedTypeSymbol Argument => _compilation.GetTypeByMetadataName(typeof(Argument<>).FullName);

		public INamedTypeSymbol Optional => _compilation.GetTypeByMetadataName(typeof(PX.Data.Optional<>).FullName);
		public INamedTypeSymbol Optional2 => _compilation.GetTypeByMetadataName(typeof(Optional2<>).FullName);

		public INamedTypeSymbol BqlCommand => _compilation.GetTypeByMetadataName(typeof(BqlCommand).FullName);

		public INamedTypeSymbol IBqlParameter => _compilation.GetTypeByMetadataName(typeof(IBqlParameter).FullName);

		public INamedTypeSymbol PXSelectBaseGenericType => _compilation.GetTypeByMetadataName(typeof(PXSelectBase<>).FullName);

		public INamedTypeSymbol PXFilter => _compilation.GetTypeByMetadataName(typeof(PXFilter<>).FullName);

		public INamedTypeSymbol IPXNonUpdateable => _compilation.GetTypeByMetadataName(typeof(IPXNonUpdateable).FullName);

		#region PXSetup
		public INamedTypeSymbol PXSetup => _compilation.GetTypeByMetadataName(typeof(PXSetup<>).FullName);

		public INamedTypeSymbol PXSetupWhere => _compilation.GetTypeByMetadataName(typeof(PXSetup<,>).FullName);

		public INamedTypeSymbol PXSetupJoin => _compilation.GetTypeByMetadataName(typeof(PXSetup<,,>).FullName);

		public INamedTypeSymbol PXSetupSelect => _compilation.GetTypeByMetadataName(typeof(PXSetupSelect<>).FullName);

		public ImmutableArray<INamedTypeSymbol> GetPXSetupTypes() =>
			ImmutableArray.Create
			(
				PXSetup,
				PXSetupWhere,
				PXSetupJoin,
				PXSetupSelect
			);
		#endregion
	}
}
