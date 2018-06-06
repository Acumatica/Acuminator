using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using PX.Data;
using Acuminator.Utilities;

namespace Acuminator.Analyzers
{
    public class PXContext
    {
        public Compilation Compilation { get; }

		private readonly Lazy<BQLSymbols> bql;

		public BQLSymbols BQL => bql.Value;

		private readonly Lazy<FieldAttributesTypes> fieldAttributes;

		public FieldAttributesTypes FieldAttributes => fieldAttributes.Value;

        public INamedTypeSymbol Array => Compilation.GetSpecialType(SpecialType.System_Array);

		public IArrayTypeSymbol ByteArray => Compilation.CreateArrayTypeSymbol(Byte);

        public INamedTypeSymbol String => Compilation.GetSpecialType(SpecialType.System_String);
        public INamedTypeSymbol Bool => Compilation.GetSpecialType(SpecialType.System_Boolean);
        public INamedTypeSymbol Int64 => Compilation.GetSpecialType(SpecialType.System_Int64);
        public INamedTypeSymbol Int32 => Compilation.GetSpecialType(SpecialType.System_Int32);
        public INamedTypeSymbol Int16 => Compilation.GetSpecialType(SpecialType.System_Int16);
        public INamedTypeSymbol Byte => Compilation.GetSpecialType(SpecialType.System_Byte);
        public INamedTypeSymbol Double => Compilation.GetSpecialType(SpecialType.System_Double);
        public INamedTypeSymbol Float => Compilation.GetSpecialType(SpecialType.System_Single);
        public INamedTypeSymbol Decimal => Compilation.GetSpecialType(SpecialType.System_Decimal);
        public INamedTypeSymbol DateTime => Compilation.GetSpecialType(SpecialType.System_DateTime);
		public INamedTypeSymbol Nullable => Compilation.GetSpecialType(SpecialType.System_Nullable_T);
		public INamedTypeSymbol Guid => Compilation.GetTypeByMetadataName(typeof(Guid).FullName);



        public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PXGraph).FullName);
        public INamedTypeSymbol PXProcessingBaseType => Compilation.GetTypeByMetadataName(typeof(PXProcessingBase<>).FullName);
        public INamedTypeSymbol PXGraphExtensionType => Compilation.GetTypeByMetadataName(typeof(PXGraphExtension).FullName);
        public INamedTypeSymbol PXCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXCacheExtension).FullName);
        public INamedTypeSymbol PXMappedCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXMappedCacheExtension).FullName);
        public INamedTypeSymbol PXViewType => Compilation.GetTypeByMetadataName(typeof(PXView).FullName);
        public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
        public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
        public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
        public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
        public INamedTypeSymbol IBqlFieldType => Compilation.GetTypeByMetadataName(typeof(IBqlField).FullName);

		public INamedTypeSymbol IPXResultsetType => Compilation.GetTypeByMetadataName(typeof(IPXResultset).FullName);
		public INamedTypeSymbol PXResult => Compilation.GetTypeByMetadataName(typeof(PXResult).FullName);

        //public INamedTypeSymbol PXBaseListAttributeType => Compilation.GetTypeByMetadataName(typeof(PXBaseListAttribute).FullName);
        public INamedTypeSymbol PXStringListAttribute => Compilation.GetTypeByMetadataName(typeof(PXStringListAttribute).FullName);
        public INamedTypeSymbol PXIntListAttribute => Compilation.GetTypeByMetadataName(typeof(PXIntListAttribute).FullName);
        public INamedTypeSymbol IPXLocalizableList => Compilation.GetTypeByMetadataName(typeof(IPXLocalizableList).FullName);

        public INamedTypeSymbol PXEventSubscriberAttribute => Compilation.GetTypeByMetadataName(typeof(PXEventSubscriberAttribute).FullName);
        public INamedTypeSymbol PXFieldState => Compilation.GetTypeByMetadataName(typeof(PXFieldState).FullName);
        public INamedTypeSymbol PXAttributeFamily => Compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);
         
        public PXContext(Compilation compilation)
        {
            Compilation = compilation;
			bql = new Lazy<BQLSymbols>(() => new BQLSymbols(Compilation)); 
            fieldAttributes = new Lazy<FieldAttributesTypes>(() => new FieldAttributesTypes(Compilation));
        }

        #region Field Attributes Types
        public class FieldAttributesTypes
        {
            private readonly Compilation compilation;

            public FieldAttributesTypes(Compilation aCompilation)
            {
                compilation = aCompilation;
            }

            #region Field Unbound Attributes
            public INamedTypeSymbol PXLongAttribute => compilation.GetTypeByMetadataName(typeof(PXLongAttribute).FullName);
            public INamedTypeSymbol PXIntAttribute => compilation.GetTypeByMetadataName(typeof(PXIntAttribute).FullName);
            public INamedTypeSymbol PXShortAttribute => compilation.GetTypeByMetadataName(typeof(PXShortAttribute).FullName);
            public INamedTypeSymbol PXStringAttribute => compilation.GetTypeByMetadataName(typeof(PXStringAttribute).FullName);
            public INamedTypeSymbol PXByteAttribute => compilation.GetTypeByMetadataName(typeof(PXByteAttribute).FullName);
            public INamedTypeSymbol PXDecimalAttribute => compilation.GetTypeByMetadataName(typeof(PXDecimalAttribute).FullName);
            public INamedTypeSymbol PXDoubleAttribute => compilation.GetTypeByMetadataName(typeof(PXDoubleAttribute).FullName);

			public INamedTypeSymbol PXFloatAttribute => compilation.GetTypeByMetadataName(typeof(PXFloatAttribute).FullName);
			public INamedTypeSymbol PXDateAttribute => compilation.GetTypeByMetadataName(typeof(PXDateAttribute).FullName);
            public INamedTypeSymbol PXGuidAttribute => compilation.GetTypeByMetadataName(typeof(PXGuidAttribute).FullName);
            public INamedTypeSymbol PXBoolAttribute => compilation.GetTypeByMetadataName(typeof(PXBoolAttribute).FullName);			
            #endregion

            #region DBField Attributes
            public INamedTypeSymbol PXDBFieldAttribute => compilation.GetTypeByMetadataName(typeof(PXDBFieldAttribute).FullName);

            public INamedTypeSymbol PXDBLongAttribute => compilation.GetTypeByMetadataName(typeof(PXDBLongAttribute).FullName);
            public INamedTypeSymbol PXDBIntAttribute => compilation.GetTypeByMetadataName(typeof(PXDBIntAttribute).FullName);
            public INamedTypeSymbol PXDBShortAttribute => compilation.GetTypeByMetadataName(typeof(PXDBShortAttribute).FullName);
            public INamedTypeSymbol PXDBStringAttribute => compilation.GetTypeByMetadataName(typeof(PXDBStringAttribute).FullName);
            public INamedTypeSymbol PXDBByteAttribute => compilation.GetTypeByMetadataName(typeof(PXDBByteAttribute).FullName);
            public INamedTypeSymbol PXDBDecimalAttribute => compilation.GetTypeByMetadataName(typeof(PXDBDecimalAttribute).FullName);
            public INamedTypeSymbol PXDBDoubleAttribute => compilation.GetTypeByMetadataName(typeof(PXDBDoubleAttribute).FullName);
            public INamedTypeSymbol PXDBFloatAttribute => compilation.GetTypeByMetadataName(typeof(PXDBFloatAttribute).FullName);
            public INamedTypeSymbol PXDBDateAttribute => compilation.GetTypeByMetadataName(typeof(PXDBDateAttribute).FullName);
            public INamedTypeSymbol PXDBGuidAttribute => compilation.GetTypeByMetadataName(typeof(PXDBGuidAttribute).FullName);
            public INamedTypeSymbol PXDBBoolAttribute => compilation.GetTypeByMetadataName(typeof(PXDBBoolAttribute).FullName);
            public INamedTypeSymbol PXDBTimestampAttribute => compilation.GetTypeByMetadataName(typeof(PXDBTimestampAttribute).FullName);

            public INamedTypeSymbol PXDBIdentityAttribute => compilation.GetTypeByMetadataName(typeof(PXDBIdentityAttribute).FullName);
            public INamedTypeSymbol PXDBLongIdentityAttribute => compilation.GetTypeByMetadataName(typeof(PXDBLongIdentityAttribute).FullName);
            public INamedTypeSymbol PXDBBinaryAttribute => compilation.GetTypeByMetadataName(typeof(PXDBBinaryAttribute).FullName);
            public INamedTypeSymbol PXDBUserPasswordAttribute => compilation.GetTypeByMetadataName(typeof(PXDBUserPasswordAttribute).FullName);
            #endregion
        }
        #endregion

		#region BQL Types
		/// <summary>
		/// BQL Symbols are stored in separate file.
		/// </summary>
		public class BQLSymbols
		{
			private readonly Compilation compilation;

			public BQLSymbols(Compilation aCompilation)
			{
				compilation = aCompilation;
			}

			public INamedTypeSymbol Required => compilation.GetTypeByMetadataName(typeof(Required<>).FullName);

			public INamedTypeSymbol Argument => compilation.GetTypeByMetadataName(typeof(Argument<>).FullName);

			public INamedTypeSymbol Optional => compilation.GetTypeByMetadataName(typeof(PX.Data.Optional<>).FullName);
			public INamedTypeSymbol Optional2 => compilation.GetTypeByMetadataName(typeof(Optional2<>).FullName);

			public INamedTypeSymbol BqlCommand => compilation.GetTypeByMetadataName(typeof(BqlCommand).FullName);

			public INamedTypeSymbol IBqlParameter => compilation.GetTypeByMetadataName(typeof(IBqlParameter).FullName);

			public INamedTypeSymbol PXSelectBaseGenericType => compilation.GetTypeByMetadataName(typeof(PXSelectBase<>).FullName);

		}
        #endregion
    }
}
