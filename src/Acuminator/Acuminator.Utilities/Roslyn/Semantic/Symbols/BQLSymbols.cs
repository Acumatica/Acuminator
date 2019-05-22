using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

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
		public INamedTypeSymbol CustomPredicate => _compilation.GetTypeByMetadataName(Types.CustomPredicate);

		public INamedTypeSymbol AreSame => _compilation.GetTypeByMetadataName(Types.AreSame2);

		public INamedTypeSymbol AreDistinct => _compilation.GetTypeByMetadataName(Types.AreDistinct2);
		#endregion

		public INamedTypeSymbol Required => _compilation.GetTypeByMetadataName(Types.Required1);

		public INamedTypeSymbol Argument => _compilation.GetTypeByMetadataName(Types.Argument1);

		public INamedTypeSymbol Optional => _compilation.GetTypeByMetadataName(Types.Optional1);
		public INamedTypeSymbol Optional2 => _compilation.GetTypeByMetadataName(Types.Optional2);

		public INamedTypeSymbol BqlCommand => _compilation.GetTypeByMetadataName(Types.BqlCommand);

		public INamedTypeSymbol IBqlParameter => _compilation.GetTypeByMetadataName(Types.IBqlParameter);

		public INamedTypeSymbol PXSelectBaseGenericType => _compilation.GetTypeByMetadataName(Types.PXSelectBase1);

		public INamedTypeSymbol PXFilter => _compilation.GetTypeByMetadataName(Types.PXFilter1);

		public INamedTypeSymbol IPXNonUpdateable => _compilation.GetTypeByMetadataName(Types.IPXNonUpdateable);

		#region PXSetup
		public INamedTypeSymbol PXSetup => _compilation.GetTypeByMetadataName(Types.PXSetup1);

		public INamedTypeSymbol PXSetupWhere => _compilation.GetTypeByMetadataName(Types.PXSetup2);

		public INamedTypeSymbol PXSetupJoin => _compilation.GetTypeByMetadataName(Types.PXSetup3);

		public INamedTypeSymbol PXSetupSelect => _compilation.GetTypeByMetadataName(Types.PXSetupSelect);

		public INamedTypeSymbol FbqlCommand => _compilation.GetTypeByMetadataName(Types.FbqlCommand);

		public INamedTypeSymbol PXViewOf => _compilation.GetTypeByMetadataName(Types.PXViewOf);

		public INamedTypeSymbol PXViewOf_BasedOn => _compilation.GetTypeByMetadataName(Types.PXViewOfBasedOn);

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
