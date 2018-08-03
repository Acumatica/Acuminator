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
		private const string PXSelectBase_Acumatica2018R2 = "PX.Data.PXSelectBase`2";
		private const string IViewConfig_Acumatica2018R2 = "PX.Data.PXSelectBase`2+IViewConfig";

		public bool IsAcumatica2018R2 { get; }

        public Compilation Compilation { get; }

		private readonly Lazy<BQLSymbols> bql;

		public BQLSymbols BQL => bql.Value;

		private readonly Lazy<FieldAttributesTypes> fieldAttributes;

		public FieldAttributesTypes FieldAttributes => fieldAttributes.Value;

		private readonly Lazy<PXSystemActionTypes> systemActionTypes;

		public PXSystemActionTypes PXSystemActions => systemActionTypes.Value;




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

        public INamedTypeSymbol PXSelectBase2018R2NewType => Compilation.GetTypeByMetadataName(PXSelectBase_Acumatica2018R2);
        public INamedTypeSymbol IViewConfig2018R2 => Compilation.GetTypeByMetadataName(IViewConfig_Acumatica2018R2);

		

        public INamedTypeSymbol PXActionType => Compilation.GetTypeByMetadataName(typeof(PXAction).FullName);
		
		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
        public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
        public INamedTypeSymbol IBqlFieldType => Compilation.GetTypeByMetadataName(typeof(IBqlField).FullName);

		public INamedTypeSymbol IPXResultsetType => Compilation.GetTypeByMetadataName(typeof(IPXResultset).FullName);
		public INamedTypeSymbol PXResult => Compilation.GetTypeByMetadataName(typeof(PXResult).FullName);

        public INamedTypeSymbol PXImportAttribute => Compilation.GetTypeByMetadataName(typeof(PXImportAttribute).FullName);
        public INamedTypeSymbol PXHiddenAttribute => Compilation.GetTypeByMetadataName(typeof(PXHiddenAttribute).FullName);
        public INamedTypeSymbol PXCopyPasteHiddenViewAttribute => Compilation.GetTypeByMetadataName(typeof(PXCopyPasteHiddenViewAttribute).FullName);
        
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
            fieldAttributes = new Lazy<FieldAttributesTypes>(
										() => new FieldAttributesTypes(Compilation));
            systemActionTypes = new Lazy<PXSystemActionTypes>(
										() => new PXSystemActionTypes(Compilation));

			IsAcumatica2018R2 = PXSelectBase2018R2NewType != null;
		}

        #region Field Attributes Types
        public class FieldAttributesTypes
        {
            private readonly Compilation compilation;

            public FieldAttributesTypes(Compilation aCompilation)
            {
                compilation = aCompilation;
            }

			public INamedTypeSymbol PXDBScalarAttribute => compilation.GetTypeByMetadataName(typeof(PXDBScalarAttribute).FullName);

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

		#region System Actions Types
		public class PXSystemActionTypes
		{
			private readonly Compilation compilation;

			public PXSystemActionTypes(Compilation aCompilation)
			{
				compilation = aCompilation;
			}

			public INamedTypeSymbol PXSave => compilation.GetTypeByMetadataName(typeof(PXSave<>).FullName);

			public INamedTypeSymbol PXCancel => compilation.GetTypeByMetadataName(typeof(PXCancel<>).FullName);

			public INamedTypeSymbol PXInsert => compilation.GetTypeByMetadataName(typeof(PXInsert<>).FullName);

			public INamedTypeSymbol PXDelete => compilation.GetTypeByMetadataName(typeof(PXDelete<>).FullName);

			public INamedTypeSymbol PXCopyPasteAction => compilation.GetTypeByMetadataName(typeof(PXCopyPasteAction<>).FullName);

			public INamedTypeSymbol PXFirst => compilation.GetTypeByMetadataName(typeof(PXFirst<>).FullName);

			public INamedTypeSymbol PXPrevious => compilation.GetTypeByMetadataName(typeof(PXPrevious<>).FullName);

			public INamedTypeSymbol PXNext => compilation.GetTypeByMetadataName(typeof(PXNext<>).FullName);

			public INamedTypeSymbol PXLast => compilation.GetTypeByMetadataName(typeof(PXLast<>).FullName);
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

			#region CustomDelegates
			public INamedTypeSymbol CustomPredicate => compilation.GetTypeByMetadataName(typeof(CustomPredicate).FullName);

			public INamedTypeSymbol AreSame => compilation.GetTypeByMetadataName(typeof(AreSame<,>).FullName);

			public INamedTypeSymbol AreDistinct => compilation.GetTypeByMetadataName(typeof(AreDistinct<,>).FullName);
			#endregion

			public INamedTypeSymbol Required => compilation.GetTypeByMetadataName(typeof(Required<>).FullName);

			public INamedTypeSymbol Argument => compilation.GetTypeByMetadataName(typeof(Argument<>).FullName);

			public INamedTypeSymbol Optional => compilation.GetTypeByMetadataName(typeof(PX.Data.Optional<>).FullName);
			public INamedTypeSymbol Optional2 => compilation.GetTypeByMetadataName(typeof(Optional2<>).FullName);

			public INamedTypeSymbol BqlCommand => compilation.GetTypeByMetadataName(typeof(BqlCommand).FullName);

			public INamedTypeSymbol IBqlParameter => compilation.GetTypeByMetadataName(typeof(IBqlParameter).FullName);

			public INamedTypeSymbol PXSelectBaseGenericType => compilation.GetTypeByMetadataName(typeof(PXSelectBase<>).FullName);

			public INamedTypeSymbol PXFilter => compilation.GetTypeByMetadataName(typeof(PXFilter<>).FullName);

			public INamedTypeSymbol IPXNonUpdateable => compilation.GetTypeByMetadataName(typeof(IPXNonUpdateable).FullName); 

			#region PXSetup
			public INamedTypeSymbol PXSetup => compilation.GetTypeByMetadataName(typeof(PXSetup<>).FullName);

			public INamedTypeSymbol PXSetupWhere => compilation.GetTypeByMetadataName(typeof(PXSetup<,>).FullName);

			public INamedTypeSymbol PXSetupJoin => compilation.GetTypeByMetadataName(typeof(PXSetup<,,>).FullName);

			public INamedTypeSymbol PXSetupSelect => compilation.GetTypeByMetadataName(typeof(PXSetupSelect<>).FullName);

			public ImmutableArray<INamedTypeSymbol> GetPXSetupTypes() =>
				ImmutableArray.Create
				(
					PXSetup,
					PXSetupWhere,
					PXSetupJoin,
					PXSetupSelect
				);
			#endregion
		}
        #endregion
    }
}
