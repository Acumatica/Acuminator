using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class BqlDataTypeSymbols
	{

		private readonly Compilation _compilation;

		public BqlDataTypeSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol BqlDataType => _compilation.GetTypeByMetadataName(TypeFullNames.BqlDataTypeType);
		public INamedTypeSymbol BqlString => _compilation.GetTypeByMetadataName(TypeFullNames.BqlStringType);
		public INamedTypeSymbol BqlGuid => _compilation.GetTypeByMetadataName(TypeFullNames.BqlGuidType);
		public INamedTypeSymbol BqlDateTime => _compilation.GetTypeByMetadataName(TypeFullNames.BqlDateTimeType);
		public INamedTypeSymbol BqlBool => _compilation.GetTypeByMetadataName(TypeFullNames.BqlBoolType);
		public INamedTypeSymbol BqlByte => _compilation.GetTypeByMetadataName(TypeFullNames.BqlByteType);
		public INamedTypeSymbol BqlShort => _compilation.GetTypeByMetadataName(TypeFullNames.BqlShortType);
		public INamedTypeSymbol BqlInt => _compilation.GetTypeByMetadataName(TypeFullNames.BqlIntType);
		public INamedTypeSymbol BqlLong => _compilation.GetTypeByMetadataName(TypeFullNames.BqlLongType);
		public INamedTypeSymbol BqlFloat => _compilation.GetTypeByMetadataName(TypeFullNames.BqlFloatType);
		public INamedTypeSymbol BqlDouble => _compilation.GetTypeByMetadataName(TypeFullNames.BqlDoubleType);
		public INamedTypeSymbol BqlDecimal => _compilation.GetTypeByMetadataName(TypeFullNames.BqlDecimalType);
		public INamedTypeSymbol BqlByteArray => _compilation.GetTypeByMetadataName(TypeFullNames.BqlByteArrayType);
	}
}