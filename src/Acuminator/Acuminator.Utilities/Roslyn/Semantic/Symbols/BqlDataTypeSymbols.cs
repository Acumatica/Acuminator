using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class BqlDataTypeSymbols : SymbolsSetBase
	{
		public BqlDataTypeSymbols(Compilation compilation) : base(compilation)
		{
		}

		public INamedTypeSymbol IBqlDataType => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlDataTypeType)!;

		public INamedTypeSymbol IBqlString => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlStringType)!;

		public INamedTypeSymbol IBqlGuid => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlGuidType)!;

		public INamedTypeSymbol IBqlDateTime => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlDateTimeType)!;

		public INamedTypeSymbol IBqlBool => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlBoolType)!;

		public INamedTypeSymbol IBqlByte => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlByteType)!;

		public INamedTypeSymbol IBqlShort => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlShortType)!;

		public INamedTypeSymbol IBqlInt => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlIntType)!;

		public INamedTypeSymbol IBqlLong => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlLongType)!;

		public INamedTypeSymbol IBqlFloat => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlFloatType)!;

		public INamedTypeSymbol IBqlDouble => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlDoubleType)!;

		public INamedTypeSymbol IBqlDecimal => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlDecimalType)!;

		public INamedTypeSymbol IBqlByteArray => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlByteArrayType)!;

		public INamedTypeSymbol? IBqlAttributes => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlAttributes);
	}
}