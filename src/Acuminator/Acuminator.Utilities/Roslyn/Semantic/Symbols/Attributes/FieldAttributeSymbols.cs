using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	public class FieldAttributeSymbols : SymbolsSetBase
	{
		internal FieldAttributeSymbols(Compilation compilation) : base(compilation)
		{ }

		#region Field Unbound Attributes
		public INamedTypeSymbol PXLongAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXLongAttribute);
		public INamedTypeSymbol PXIntAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXIntAttribute);
		public INamedTypeSymbol PXShortAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXShortAttribute);
		public INamedTypeSymbol PXStringAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXStringAttribute);
		public INamedTypeSymbol PXByteAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXByteAttribute);
		public INamedTypeSymbol PXDecimalAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDecimalAttribute);
		public INamedTypeSymbol PXDoubleAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDoubleAttribute);

		public INamedTypeSymbol PXFloatAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXFloatAttribute);
		public INamedTypeSymbol PXDateAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDateAttribute);
		public INamedTypeSymbol PXGuidAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXGuidAttribute);
		public INamedTypeSymbol PXBoolAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXBoolAttribute);
		#endregion

		#region DBField Attributes
		public INamedTypeSymbol PXDBFieldAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBFieldAttribute);

		public INamedTypeSymbol PXDBLongAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBLongAttribute);
		public INamedTypeSymbol PXDBIntAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBIntAttribute);
		public INamedTypeSymbol PXDBShortAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBShortAttribute);
		public INamedTypeSymbol PXDBStringAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBStringAttribute);
		public INamedTypeSymbol PXDBLocalizableStringAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBLocalizableStringAttribute);
		public INamedTypeSymbol PXDBByteAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBByteAttribute);
		public INamedTypeSymbol PXDBDecimalAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBDecimalAttribute);
		public INamedTypeSymbol PXDBDoubleAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBDoubleAttribute);
		public INamedTypeSymbol PXDBFloatAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBFloatAttribute);
		public INamedTypeSymbol PXDBDateAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBDateAttribute);
		public INamedTypeSymbol PXDBGuidAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBGuidAttribute);
		public INamedTypeSymbol PXDBBoolAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBBoolAttribute);
		public INamedTypeSymbol PXDBTimestampAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBTimestampAttribute);

		public INamedTypeSymbol PXDBIdentityAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBIdentityAttribute);
		public INamedTypeSymbol PXDBLongIdentityAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBLongIdentityAttribute);
		public INamedTypeSymbol PXDBBinaryAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBBinaryAttribute);

		public INamedTypeSymbol PXDBPackedIntegerArrayAttribute =>
			Compilation.GetTypeByMetadataName(TypeFullNames.PXDBPackedIntegerArrayAttributeFullName_Acumatica2018R2);

		public INamedTypeSymbol PXDBUserPasswordAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBUserPasswordAttribute);

		public INamedTypeSymbol PXDBAttributeAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBAttributeAttribute);
		public INamedTypeSymbol PXDBDataLengthAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBDataLengthAttribute);
		#endregion

		#region Special attributes
		public INamedTypeSymbol PXDBCalcedAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBCalcedAttribute);
		public INamedTypeSymbol PXDBScalarAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PXDBScalarAttribute);
		#endregion

		#region DBBound  attributes defined by IsDBField 

		public INamedTypeSymbol PeriodIDAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.PeriodIDAttribute);
		public INamedTypeSymbol AcctSubAttribute => Compilation.GetTypeByMetadataName(TypeFullNames.AcctSubAttribute);

		#endregion
	}
}