using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// BQL Symbols are stored in separate file.
	/// </summary>
	public class BQLSymbols : SymbolsSetBase
	{
		#region PXSetup
		public ImmutableArray<INamedTypeSymbol> PXSetupTypes { get; }

		public INamedTypeSymbol PXSetup => _compilation.GetTypeByMetadataName(TypeFullNames.PXSetup1);

		public INamedTypeSymbol PXSetupWhere => _compilation.GetTypeByMetadataName(TypeFullNames.PXSetup2);

		public INamedTypeSymbol PXSetupJoin => _compilation.GetTypeByMetadataName(TypeFullNames.PXSetup3);

		public INamedTypeSymbol PXSetupSelect => _compilation.GetTypeByMetadataName(TypeFullNames.PXSetupSelect);
		#endregion

		#region CustomDelegates
		public INamedTypeSymbol CustomPredicate => _compilation.GetTypeByMetadataName(TypeFullNames.CustomPredicate);

		public INamedTypeSymbol AreSame => _compilation.GetTypeByMetadataName(TypeFullNames.AreSame2);

		public INamedTypeSymbol AreDistinct => _compilation.GetTypeByMetadataName(TypeFullNames.AreDistinct2);
		#endregion

		public INamedTypeSymbol Required => _compilation.GetTypeByMetadataName(TypeFullNames.Required1);

		public INamedTypeSymbol Argument => _compilation.GetTypeByMetadataName(TypeFullNames.Argument1);

		public INamedTypeSymbol Optional => _compilation.GetTypeByMetadataName(TypeFullNames.Optional1);
		public INamedTypeSymbol Optional2 => _compilation.GetTypeByMetadataName(TypeFullNames.Optional2);

		public INamedTypeSymbol BqlCommand => _compilation.GetTypeByMetadataName(TypeFullNames.BqlCommand);

		public INamedTypeSymbol IBqlParameter => _compilation.GetTypeByMetadataName(TypeFullNames.IBqlParameter);

		public INamedTypeSymbol PXSelectBaseGenericType => _compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase1);

		public INamedTypeSymbol PXFilter => _compilation.GetTypeByMetadataName(TypeFullNames.PXFilter1);

		public INamedTypeSymbol IPXNonUpdateable => _compilation.GetTypeByMetadataName(TypeFullNames.IPXNonUpdateable);

		public INamedTypeSymbol FbqlCommand => _compilation.GetTypeByMetadataName(TypeFullNames.FbqlCommand);

		public INamedTypeSymbol PXViewOf => _compilation.GetTypeByMetadataName(TypeFullNames.PXViewOf);

		public INamedTypeSymbol PXViewOf_BasedOn => _compilation.GetTypeByMetadataName(TypeFullNames.PXViewOfBasedOn);

		internal BQLSymbols(Compilation compilation) : base(compilation)
		{
			PXSetupTypes = ImmutableArray.Create
			(
				PXSetup,
				PXSetupWhere,
				PXSetupJoin,
				PXSetupSelect
			);
		}
	}
}
