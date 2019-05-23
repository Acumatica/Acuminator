using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class BqlDataTypeSymbols
	{

		private readonly Compilation _compilation;

		public BqlDataTypeSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol BqlDataType => _compilation.GetTypeByMetadataName(Types.BqlDataTypeType);
		public INamedTypeSymbol BqlString => _compilation.GetTypeByMetadataName(Types.BqlStringType);
		public INamedTypeSymbol BqlGuid => _compilation.GetTypeByMetadataName(Types.BqlGuidType);
		public INamedTypeSymbol BqlDateTime => _compilation.GetTypeByMetadataName(Types.BqlDateTimeType);
		public INamedTypeSymbol BqlBool => _compilation.GetTypeByMetadataName(Types.BqlBoolType);
		public INamedTypeSymbol BqlByte => _compilation.GetTypeByMetadataName(Types.BqlByteType);
		public INamedTypeSymbol BqlShort => _compilation.GetTypeByMetadataName(Types.BqlShortType);
		public INamedTypeSymbol BqlInt => _compilation.GetTypeByMetadataName(Types.BqlIntType);
		public INamedTypeSymbol BqlLong => _compilation.GetTypeByMetadataName(Types.BqlLongType);
		public INamedTypeSymbol BqlFloat => _compilation.GetTypeByMetadataName(Types.BqlFloatType);
		public INamedTypeSymbol BqlDouble => _compilation.GetTypeByMetadataName(Types.BqlDoubleType);
		public INamedTypeSymbol BqlDecimal => _compilation.GetTypeByMetadataName(Types.BqlDecimalType);
		public INamedTypeSymbol BqlByteArray => _compilation.GetTypeByMetadataName(Types.BqlByteArrayType);
	}
}