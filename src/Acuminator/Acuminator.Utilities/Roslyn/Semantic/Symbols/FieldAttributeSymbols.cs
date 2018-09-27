using Microsoft.CodeAnalysis;
using PX.Data;

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
		public INamedTypeSymbol PXLongAttribute => _compilation.GetTypeByMetadataName(typeof(PXLongAttribute).FullName);
		public INamedTypeSymbol PXIntAttribute => _compilation.GetTypeByMetadataName(typeof(PXIntAttribute).FullName);
		public INamedTypeSymbol PXShortAttribute => _compilation.GetTypeByMetadataName(typeof(PXShortAttribute).FullName);
		public INamedTypeSymbol PXStringAttribute => _compilation.GetTypeByMetadataName(typeof(PXStringAttribute).FullName);
		public INamedTypeSymbol PXByteAttribute => _compilation.GetTypeByMetadataName(typeof(PXByteAttribute).FullName);
		public INamedTypeSymbol PXDecimalAttribute => _compilation.GetTypeByMetadataName(typeof(PXDecimalAttribute).FullName);
		public INamedTypeSymbol PXDoubleAttribute => _compilation.GetTypeByMetadataName(typeof(PXDoubleAttribute).FullName);

		public INamedTypeSymbol PXFloatAttribute => _compilation.GetTypeByMetadataName(typeof(PXFloatAttribute).FullName);
		public INamedTypeSymbol PXDateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDateAttribute).FullName);
		public INamedTypeSymbol PXGuidAttribute => _compilation.GetTypeByMetadataName(typeof(PXGuidAttribute).FullName);
		public INamedTypeSymbol PXBoolAttribute => _compilation.GetTypeByMetadataName(typeof(PXBoolAttribute).FullName);
		#endregion

		#region DBField Attributes
		public INamedTypeSymbol PXDBFieldAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBFieldAttribute).FullName);

		public INamedTypeSymbol PXDBLongAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBLongAttribute).FullName);
		public INamedTypeSymbol PXDBIntAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBIntAttribute).FullName);
		public INamedTypeSymbol PXDBShortAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBShortAttribute).FullName);
		public INamedTypeSymbol PXDBStringAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBStringAttribute).FullName);
		public INamedTypeSymbol PXDBByteAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBByteAttribute).FullName);
		public INamedTypeSymbol PXDBDecimalAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDecimalAttribute).FullName);
		public INamedTypeSymbol PXDBDoubleAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDoubleAttribute).FullName);
		public INamedTypeSymbol PXDBFloatAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBFloatAttribute).FullName);
		public INamedTypeSymbol PXDBDateAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDateAttribute).FullName);
		public INamedTypeSymbol PXDBGuidAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBGuidAttribute).FullName);
		public INamedTypeSymbol PXDBBoolAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBBoolAttribute).FullName);
		public INamedTypeSymbol PXDBTimestampAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBTimestampAttribute).FullName);

		public INamedTypeSymbol PXDBIdentityAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBIdentityAttribute).FullName);
		public INamedTypeSymbol PXDBLongIdentityAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBLongIdentityAttribute).FullName);
		public INamedTypeSymbol PXDBBinaryAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBBinaryAttribute).FullName);
		public INamedTypeSymbol PXDBUserPasswordAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBUserPasswordAttribute).FullName);

		public INamedTypeSymbol PXDBAttributeAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBAttributeAttribute).FullName);
		public INamedTypeSymbol PXDBDataLengthAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBDataLengthAttribute).FullName);
		#endregion

		#region Special attributes
		public INamedTypeSymbol PXDBCalcedAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBCalcedAttribute).FullName);
		public INamedTypeSymbol PXDBScalarAttribute => _compilation.GetTypeByMetadataName(typeof(PXDBScalarAttribute).FullName);
		#endregion
	}
}
