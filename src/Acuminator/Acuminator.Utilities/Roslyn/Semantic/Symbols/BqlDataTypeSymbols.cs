using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class BqlDataTypeSymbols : SymbolsSetBase
	{
		public BqlDataTypeSymbols(Compilation compilation) : base(compilation)
		{
		}

		public INamedTypeSymbol BqlDataType => Compilation.GetTypeByMetadataName(TypeFullNames.BqlDataTypeType);
		public INamedTypeSymbol BqlString => Compilation.GetTypeByMetadataName(TypeFullNames.BqlStringType);
		public INamedTypeSymbol BqlGuid => Compilation.GetTypeByMetadataName(TypeFullNames.BqlGuidType);
		public INamedTypeSymbol BqlDateTime => Compilation.GetTypeByMetadataName(TypeFullNames.BqlDateTimeType);
		public INamedTypeSymbol BqlBool => Compilation.GetTypeByMetadataName(TypeFullNames.BqlBoolType);
		public INamedTypeSymbol BqlByte => Compilation.GetTypeByMetadataName(TypeFullNames.BqlByteType);
		public INamedTypeSymbol BqlShort => Compilation.GetTypeByMetadataName(TypeFullNames.BqlShortType);
		public INamedTypeSymbol BqlInt => Compilation.GetTypeByMetadataName(TypeFullNames.BqlIntType);
		public INamedTypeSymbol BqlLong => Compilation.GetTypeByMetadataName(TypeFullNames.BqlLongType);
		public INamedTypeSymbol BqlFloat => Compilation.GetTypeByMetadataName(TypeFullNames.BqlFloatType);
		public INamedTypeSymbol BqlDouble => Compilation.GetTypeByMetadataName(TypeFullNames.BqlDoubleType);
		public INamedTypeSymbol BqlDecimal => Compilation.GetTypeByMetadataName(TypeFullNames.BqlDecimalType);
		public INamedTypeSymbol BqlByteArray => Compilation.GetTypeByMetadataName(TypeFullNames.BqlByteArrayType);
	}
}