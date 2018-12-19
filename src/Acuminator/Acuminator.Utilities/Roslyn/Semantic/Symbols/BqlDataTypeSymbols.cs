using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class BqlDataTypeSymbols
	{
		private const string BqlDataTypeType = "PX.Data.BQL.IBqlDataType";
		private const string BqlStringType = "PX.Data.BQL.IBqlString";
		private const string BqlGuidType = "PX.Data.BQL.IBqlGuid";
		private const string BqlDateTimeType = "PX.Data.BQL.IBqlDateTime";
		private const string BqlBoolType = "PX.Data.BQL.IBqlBool";
		private const string BqlByteType = "PX.Data.BQL.IBqlByte";
		private const string BqlShortType = "PX.Data.BQL.IBqlShort";
		private const string BqlIntType = "PX.Data.BQL.IBqlInt";
		private const string BqlLongType = "PX.Data.BQL.IBqlLong";
		private const string BqlFloatType = "PX.Data.BQL.IBqlFloat";
		private const string BqlDoubleType = "PX.Data.BQL.IBqlDouble";
		private const string BqlDecimalType = "PX.Data.BQL.IBqlDecimal";
		private const string BqlByteArrayType = "PX.Data.BQL.IBqlByteArray";

		private readonly Compilation _compilation;

		public BqlDataTypeSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		public INamedTypeSymbol BqlDataType => _compilation.GetTypeByMetadataName(BqlDataTypeType);
		public INamedTypeSymbol BqlString => _compilation.GetTypeByMetadataName(BqlStringType);
		public INamedTypeSymbol BqlGuid => _compilation.GetTypeByMetadataName(BqlGuidType);
		public INamedTypeSymbol BqlDateTime => _compilation.GetTypeByMetadataName(BqlDateTimeType);
		public INamedTypeSymbol BqlBool => _compilation.GetTypeByMetadataName(BqlBoolType);
		public INamedTypeSymbol BqlByte => _compilation.GetTypeByMetadataName(BqlByteType);
		public INamedTypeSymbol BqlShort => _compilation.GetTypeByMetadataName(BqlShortType);
		public INamedTypeSymbol BqlInt => _compilation.GetTypeByMetadataName(BqlIntType);
		public INamedTypeSymbol BqlLong => _compilation.GetTypeByMetadataName(BqlLongType);
		public INamedTypeSymbol BqlFloat => _compilation.GetTypeByMetadataName(BqlFloatType);
		public INamedTypeSymbol BqlDouble => _compilation.GetTypeByMetadataName(BqlDoubleType);
		public INamedTypeSymbol BqlDecimal => _compilation.GetTypeByMetadataName(BqlDecimalType);
		public INamedTypeSymbol BqlByteArray => _compilation.GetTypeByMetadataName(BqlByteArrayType);
	}
}