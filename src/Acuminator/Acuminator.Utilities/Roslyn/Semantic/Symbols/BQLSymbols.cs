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

		public INamedTypeSymbol PXSetup => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetup1);

		public INamedTypeSymbol PXSetupWhere => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetup2);

		public INamedTypeSymbol PXSetupJoin => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetup3);

		public INamedTypeSymbol PXSetupSelect => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetupSelect);
		#endregion

		#region CustomDelegates
		public INamedTypeSymbol CustomPredicate => Compilation.GetTypeByMetadataName(TypeFullNames.CustomPredicate);

		public INamedTypeSymbol AreSame => Compilation.GetTypeByMetadataName(TypeFullNames.AreSame2);

		public INamedTypeSymbol AreDistinct => Compilation.GetTypeByMetadataName(TypeFullNames.AreDistinct2);
		#endregion

		public INamedTypeSymbol Required => Compilation.GetTypeByMetadataName(TypeFullNames.Required1);

		public INamedTypeSymbol Argument => Compilation.GetTypeByMetadataName(TypeFullNames.Argument1);

		public INamedTypeSymbol Optional => Compilation.GetTypeByMetadataName(TypeFullNames.Optional1);
		public INamedTypeSymbol Optional2 => Compilation.GetTypeByMetadataName(TypeFullNames.Optional2);

		public INamedTypeSymbol BqlCommand => Compilation.GetTypeByMetadataName(TypeFullNames.BqlCommand);

		public INamedTypeSymbol IBqlParameter => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlParameter);

		public INamedTypeSymbol BqlParameter => Compilation.GetTypeByMetadataName(TypeFullNames.BqlParameter);

		public INamedTypeSymbol PXSelectBaseGenericType => Compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase1);

		public INamedTypeSymbol PXFilter => Compilation.GetTypeByMetadataName(TypeFullNames.PXFilter1);

		public INamedTypeSymbol IPXNonUpdateable => Compilation.GetTypeByMetadataName(TypeFullNames.IPXNonUpdateable);

		public INamedTypeSymbol FbqlCommand => Compilation.GetTypeByMetadataName(TypeFullNames.FbqlCommand);

		public INamedTypeSymbol PXViewOf => Compilation.GetTypeByMetadataName(TypeFullNames.PXViewOf);

		public INamedTypeSymbol PXViewOf_BasedOn => Compilation.GetTypeByMetadataName(TypeFullNames.PXViewOfBasedOn);

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
