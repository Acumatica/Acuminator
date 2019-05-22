using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class FieldAttributeSymbols
	{
		private readonly Compilation _compilation;

		internal FieldAttributeSymbols(Compilation aCompilation)
		{
			_compilation = aCompilation;
		}

		#region Field Unbound Attributes
		public INamedTypeSymbol PXLongAttribute => _compilation.GetTypeByMetadataName(Types.PXLongAttribute);
		public INamedTypeSymbol PXIntAttribute => _compilation.GetTypeByMetadataName(Types.PXIntAttribute);
		public INamedTypeSymbol PXShortAttribute => _compilation.GetTypeByMetadataName(Types.PXShortAttribute);
		public INamedTypeSymbol PXStringAttribute => _compilation.GetTypeByMetadataName(Types.PXStringAttribute);
		public INamedTypeSymbol PXByteAttribute => _compilation.GetTypeByMetadataName(Types.PXByteAttribute);
		public INamedTypeSymbol PXDecimalAttribute => _compilation.GetTypeByMetadataName(Types.PXDecimalAttribute);
		public INamedTypeSymbol PXDoubleAttribute => _compilation.GetTypeByMetadataName(Types.PXDoubleAttribute);

		public INamedTypeSymbol PXFloatAttribute => _compilation.GetTypeByMetadataName(Types.PXFloatAttribute);
		public INamedTypeSymbol PXDateAttribute => _compilation.GetTypeByMetadataName(Types.PXDateAttribute);
		public INamedTypeSymbol PXGuidAttribute => _compilation.GetTypeByMetadataName(Types.PXGuidAttribute);
		public INamedTypeSymbol PXBoolAttribute => _compilation.GetTypeByMetadataName(Types.PXBoolAttribute);
		#endregion

		#region DBField Attributes
		public INamedTypeSymbol PXDBFieldAttribute => _compilation.GetTypeByMetadataName(Types.PXDBFieldAttribute);

		public INamedTypeSymbol PXDBLongAttribute => _compilation.GetTypeByMetadataName(Types.PXDBLongAttribute);
		public INamedTypeSymbol PXDBIntAttribute => _compilation.GetTypeByMetadataName(Types.PXDBIntAttribute);
		public INamedTypeSymbol PXDBShortAttribute => _compilation.GetTypeByMetadataName(Types.PXDBShortAttribute);
		public INamedTypeSymbol PXDBStringAttribute => _compilation.GetTypeByMetadataName(Types.PXDBStringAttribute);
		public INamedTypeSymbol PXDBByteAttribute => _compilation.GetTypeByMetadataName(Types.PXDBByteAttribute);
		public INamedTypeSymbol PXDBDecimalAttribute => _compilation.GetTypeByMetadataName(Types.PXDBDecimalAttribute);
		public INamedTypeSymbol PXDBDoubleAttribute => _compilation.GetTypeByMetadataName(Types.PXDBDoubleAttribute);
		public INamedTypeSymbol PXDBFloatAttribute => _compilation.GetTypeByMetadataName(Types.PXDBFloatAttribute);
		public INamedTypeSymbol PXDBDateAttribute => _compilation.GetTypeByMetadataName(Types.PXDBDateAttribute);
		public INamedTypeSymbol PXDBGuidAttribute => _compilation.GetTypeByMetadataName(Types.PXDBGuidAttribute);
		public INamedTypeSymbol PXDBBoolAttribute => _compilation.GetTypeByMetadataName(Types.PXDBBoolAttribute);
		public INamedTypeSymbol PXDBTimestampAttribute => _compilation.GetTypeByMetadataName(Types.PXDBTimestampAttribute);

		public INamedTypeSymbol PXDBIdentityAttribute => _compilation.GetTypeByMetadataName(Types.PXDBIdentityAttribute);
		public INamedTypeSymbol PXDBLongIdentityAttribute => _compilation.GetTypeByMetadataName(Types.PXDBLongIdentityAttribute);
		public INamedTypeSymbol PXDBBinaryAttribute => _compilation.GetTypeByMetadataName(Types.PXDBBinaryAttribute);

		public INamedTypeSymbol PXDBPackedIntegerArrayAttribute =>
			_compilation.GetTypeByMetadataName(Types.PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2);

		public INamedTypeSymbol PXDBUserPasswordAttribute => _compilation.GetTypeByMetadataName(Types.PXDBUserPasswordAttribute);

		public INamedTypeSymbol PXDBAttributeAttribute => _compilation.GetTypeByMetadataName(Types.PXDBAttributeAttribute);
		public INamedTypeSymbol PXDBDataLengthAttribute => _compilation.GetTypeByMetadataName(Types.PXDBDataLengthAttribute);
		#endregion

		#region Special attributes
		public INamedTypeSymbol PXDBCalcedAttribute => _compilation.GetTypeByMetadataName(Types.PXDBCalcedAttribute);
		public INamedTypeSymbol PXDBScalarAttribute => _compilation.GetTypeByMetadataName(Types.PXDBScalarAttribute);
		#endregion

		#region DBBound  attributes defined by IsDBField 

		public INamedTypeSymbol PeriodIDAttribute => _compilation.GetTypeByMetadataName(Types.PeriodIDAttribute);
		public INamedTypeSymbol AcctSubAttribute => _compilation.GetTypeByMetadataName(Types.AcctSubAttribute);

		#endregion
	}
}