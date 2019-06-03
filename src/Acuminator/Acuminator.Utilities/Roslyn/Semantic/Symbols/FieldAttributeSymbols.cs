using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

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
		public INamedTypeSymbol PXLongAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXLongAttribute);
		public INamedTypeSymbol PXIntAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXIntAttribute);
		public INamedTypeSymbol PXShortAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXShortAttribute);
		public INamedTypeSymbol PXStringAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXStringAttribute);
		public INamedTypeSymbol PXByteAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXByteAttribute);
		public INamedTypeSymbol PXDecimalAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDecimalAttribute);
		public INamedTypeSymbol PXDoubleAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDoubleAttribute);

		public INamedTypeSymbol PXFloatAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXFloatAttribute);
		public INamedTypeSymbol PXDateAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDateAttribute);
		public INamedTypeSymbol PXGuidAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXGuidAttribute);
		public INamedTypeSymbol PXBoolAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXBoolAttribute);
		#endregion

		#region DBField Attributes
		public INamedTypeSymbol PXDBFieldAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBFieldAttribute);

		public INamedTypeSymbol PXDBLongAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBLongAttribute);
		public INamedTypeSymbol PXDBIntAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBIntAttribute);
		public INamedTypeSymbol PXDBShortAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBShortAttribute);
		public INamedTypeSymbol PXDBStringAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBStringAttribute);
		public INamedTypeSymbol PXDBByteAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBByteAttribute);
		public INamedTypeSymbol PXDBDecimalAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBDecimalAttribute);
		public INamedTypeSymbol PXDBDoubleAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBDoubleAttribute);
		public INamedTypeSymbol PXDBFloatAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBFloatAttribute);
		public INamedTypeSymbol PXDBDateAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBDateAttribute);
		public INamedTypeSymbol PXDBGuidAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBGuidAttribute);
		public INamedTypeSymbol PXDBBoolAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBBoolAttribute);
		public INamedTypeSymbol PXDBTimestampAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBTimestampAttribute);

		public INamedTypeSymbol PXDBIdentityAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBIdentityAttribute);
		public INamedTypeSymbol PXDBLongIdentityAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBLongIdentityAttribute);
		public INamedTypeSymbol PXDBBinaryAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBBinaryAttribute);

		public INamedTypeSymbol PXDBPackedIntegerArrayAttribute =>
			_compilation.GetTypeByMetadataName(TypeFullNames.PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2);

		public INamedTypeSymbol PXDBUserPasswordAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBUserPasswordAttribute);

		public INamedTypeSymbol PXDBAttributeAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBAttributeAttribute);
		public INamedTypeSymbol PXDBDataLengthAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBDataLengthAttribute);
		#endregion

		#region Special attributes
		public INamedTypeSymbol PXDBCalcedAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBCalcedAttribute);
		public INamedTypeSymbol PXDBScalarAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PXDBScalarAttribute);
		#endregion

		#region DBBound  attributes defined by IsDBField 

		public INamedTypeSymbol PeriodIDAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.PeriodIDAttribute);
		public INamedTypeSymbol AcctSubAttribute => _compilation.GetTypeByMetadataName(TypeFullNames.AcctSubAttribute);

		#endregion
	}
}