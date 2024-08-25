#nullable enable

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public class PXContext
	{
		public bool IsAcumatica2018R2_OrGreater { get; }
		public bool IsAcumatica2019R1_OrGreater { get; }

		public bool IsAcumatica2023R1_OrGreater => PXCache.RowSelectingWhileReading != null;

		public bool IsAcumatica2024R1_OrGreater => PXBqlTable != null;

		public CodeAnalysisSettings CodeAnalysisSettings { get; }

		/// <summary>
		/// Is platform referenced in the current solution. If not then diagnostic can't run on the solution.
		/// </summary>
		public bool IsPlatformReferenced { get; }

		public Compilation Compilation { get; }

		private readonly Lazy<BQLSymbols> _bql;
		public BQLSymbols BQL => _bql.Value;

		private readonly Lazy<BqlDataTypeSymbols> _bqlTypes;
		public BqlDataTypeSymbols BqlTypes => _bqlTypes.Value;

		private readonly Lazy<EventSymbols> _events;
		public EventSymbols Events => _events.Value;

		private readonly Lazy<DataTypeAttributeSymbols> _fieldAttributes;
		public DataTypeAttributeSymbols FieldAttributes => _fieldAttributes.Value;

		private readonly Lazy<PXSystemActionSymbols> _systemActionTypes;
		public PXSystemActionSymbols PXSystemActions => _systemActionTypes.Value;

		private readonly Lazy<SystemTypeSymbols> _systemTypes;
		public SystemTypeSymbols SystemTypes => _systemTypes.Value;
		private readonly Lazy<AttributeSymbols> _attributes;
		public AttributeSymbols AttributeTypes => _attributes.Value;

        private readonly Lazy<LocalizationSymbols> _localizationMethods;
        public LocalizationSymbols Localization => _localizationMethods.Value;

        private readonly Lazy<PXGraphSymbols> _pxGraph;
        public PXGraphSymbols PXGraph => _pxGraph.Value;

        private readonly Lazy<PXCacheSymbols> _pxCache;
        public PXCacheSymbols PXCache => _pxCache.Value;

		private readonly Lazy<PXActionSymbols> _pxAction;
		public PXActionSymbols PXAction => _pxAction.Value;

        private readonly Lazy<PXSelectBaseGenericSymbols> _pxSelectBaseGeneric;
        public PXSelectBaseGenericSymbols PXSelectBaseGeneric => _pxSelectBaseGeneric.Value;

        private readonly Lazy<PXSelectBaseSymbols> _pxSelectBase;
        public PXSelectBaseSymbols PXSelectBase => _pxSelectBase.Value;

		private readonly Lazy<PXSelectExtensionSymbols> _pxSelectExtensionSymbols;

		public PXSelectExtensionSymbols PXSelectExtensionSymbols => _pxSelectExtensionSymbols.Value;

		private readonly Lazy<PXDatabaseSymbols> _pxDatabase;
		public PXDatabaseSymbols PXDatabase => _pxDatabase.Value;

		private readonly Lazy<PXViewSymbols> _pxView;
		public PXViewSymbols PXView => _pxView.Value;

		private readonly Lazy<ExceptionSymbols> _exceptions;
		public ExceptionSymbols Exceptions => _exceptions.Value;

		private readonly Lazy<SerializationSymbols> _serialization;
		public SerializationSymbols Serialization => _serialization.Value;

		private readonly Lazy<PXProcessingBaseSymbols> _pxProcessingBase;
        public PXProcessingBaseSymbols PXProcessingBase => _pxProcessingBase.Value;

		private readonly Lazy<PXReferentialIntegritySymbols> _referentialIntegritySymbols;
		public PXReferentialIntegritySymbols ReferentialIntegritySymbols => _referentialIntegritySymbols.Value;

		private readonly Lazy<ImmutableHashSet<IMethodSymbol>> _uiPresentationLogicMethods;
		public ImmutableHashSet<IMethodSymbol> UiPresentationLogicMethods => _uiPresentationLogicMethods.Value;

		private readonly Lazy<PXGraphExtensionSymbols> _pxGraphExtensionSymbols;
		public PXGraphExtensionSymbols PXGraphExtension => _pxGraphExtensionSymbols.Value;

		public INamedTypeSymbol PXCacheExtensionType => Compilation.GetTypeByMetadataName(TypeFullNames.PXCacheExtension)!;
		public INamedTypeSymbol PXMappedCacheExtensionType => Compilation.GetTypeByMetadataName(TypeFullNames.PXMappedCacheExtension)!;
		public INamedTypeSymbol PXLongOperation => Compilation.GetTypeByMetadataName(TypeFullNames.PXLongOperation)!;

		public INamedTypeSymbol PXSelectBase2018R2NewType => Compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase_Acumatica2018R2)!;
		public INamedTypeSymbol IViewConfig2018R2 => Compilation.GetTypeByMetadataName(TypeFullNames.IViewConfig_Acumatica2018R2)!;

		public INamedTypeSymbol PXActionCollection => Compilation.GetTypeByMetadataName(TypeFullNames.PXActionCollection)!;

		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(TypeFullNames.PXAdapter)!;
		public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlTable)!;
		public INamedTypeSymbol IBqlFieldType => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlField)!;

		[MemberNotNullWhen(returnValue: true, nameof(IsAcumatica2024R1_OrGreater))]
		public INamedTypeSymbol? PXBqlTable => Compilation.GetTypeByMetadataName(TypeFullNames.PXBqlTable);
		public INamedTypeSymbol? BqlConstantType => Compilation.GetTypeByMetadataName(TypeFullNames.Constant);

		public INamedTypeSymbol IPXResultsetType => Compilation.GetTypeByMetadataName(TypeFullNames.IPXResultset)!;
		public INamedTypeSymbol PXResult => Compilation.GetTypeByMetadataName(TypeFullNames.PXResult)!;

		public INamedTypeSymbol PXFieldState => Compilation.GetTypeByMetadataName(TypeFullNames.PXFieldState)!;
		public INamedTypeSymbol PXAttributeFamily => Compilation.GetTypeByMetadataName(TypeFullNames.PXAttributeFamilyAttribute)!;

        public INamedTypeSymbol IPXLocalizableList => Compilation.GetTypeByMetadataName(TypeFullNames.IPXLocalizableList)!;
		public INamedTypeSymbol PXConnectionScope => Compilation.GetTypeByMetadataName(TypeFullNames.PXConnectionScope)!;

        public ImmutableArray<IMethodSymbol> StartOperation => PXLongOperation.GetMethods(DelegateNames.StartOperation).ToImmutableArray();

		public INamedTypeSymbol IImplementType => Compilation.GetTypeByMetadataName(TypeFullNames.IImplementType)!;

		public INamedTypeSymbol? PXScreenConfiguration => Compilation.GetTypeByMetadataName(TypeFullNames.Workflow.PXScreenConfiguration);

		public PXContext(Compilation compilation, CodeAnalysisSettings? codeAnalysisSettings)
		{
			compilation.ThrowOnNull();

			CodeAnalysisSettings = codeAnalysisSettings ?? CodeAnalysisSettings.Default;
			Compilation = compilation;
			IsPlatformReferenced = compilation.GetTypeByMetadataName(TypeFullNames.PXGraph) != null;

			_bql                         = new Lazy<BQLSymbols>(() => new BQLSymbols(Compilation));
			_bqlTypes                    = new Lazy<BqlDataTypeSymbols>(() => new BqlDataTypeSymbols(Compilation));
			_events                      = new Lazy<EventSymbols>(() => new EventSymbols(Compilation));
			_fieldAttributes             = new Lazy<DataTypeAttributeSymbols>(() => new DataTypeAttributeSymbols(Compilation));
			_systemActionTypes           = new Lazy<PXSystemActionSymbols>(() => new PXSystemActionSymbols(Compilation));
			_attributes                  = new Lazy<AttributeSymbols>(() => new AttributeSymbols(Compilation));
			_systemTypes                 = new Lazy<SystemTypeSymbols>(() => new SystemTypeSymbols(Compilation));
            _localizationMethods         = new Lazy<LocalizationSymbols>(() => new LocalizationSymbols(Compilation));
            _pxGraph                     = new Lazy<PXGraphSymbols>(() => new PXGraphSymbols(this));
			_pxGraphExtensionSymbols     = new Lazy<PXGraphExtensionSymbols>(() => new PXGraphExtensionSymbols(this));
            _pxCache                     = new Lazy<PXCacheSymbols>(() => new PXCacheSymbols(Compilation));
			_pxAction                    = new Lazy<PXActionSymbols>(() => new PXActionSymbols(Compilation));
			_pxDatabase                  = new Lazy<PXDatabaseSymbols>(() => new PXDatabaseSymbols(Compilation));
			_pxView                      = new Lazy<PXViewSymbols>(() => new PXViewSymbols(Compilation));
			_exceptions                  = new Lazy<ExceptionSymbols>(() => new ExceptionSymbols(Compilation));
			_serialization               = new Lazy<SerializationSymbols>(() => new SerializationSymbols(Compilation));
            _pxSelectBaseGeneric         = new Lazy<PXSelectBaseGenericSymbols>(() => new PXSelectBaseGenericSymbols(Compilation));
            _pxSelectBase                = new Lazy<PXSelectBaseSymbols>(() => new PXSelectBaseSymbols(Compilation));
			_pxSelectExtensionSymbols    = new Lazy<PXSelectExtensionSymbols>(() => new PXSelectExtensionSymbols(Compilation));
            _pxProcessingBase            = new Lazy<PXProcessingBaseSymbols>(() => new PXProcessingBaseSymbols(Compilation));
            _referentialIntegritySymbols = new Lazy<PXReferentialIntegritySymbols>(() => new PXReferentialIntegritySymbols(Compilation));

			_uiPresentationLogicMethods = new Lazy<ImmutableHashSet<IMethodSymbol>>(GetUiPresentationLogicMethods);

            IsAcumatica2018R2_OrGreater = PXSelectBase2018R2NewType != null;
			IsAcumatica2019R1_OrGreater = IImplementType != null;
		}

		private ImmutableHashSet<IMethodSymbol> GetUiPresentationLogicMethods()
		{
			return PXAction.SetVisible
				.Concat(PXAction.SetEnabled)
				.Concat(PXAction.SetCaption)
				.Concat(PXAction.SetTooltip)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetVisible)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetVisibility)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetEnabled)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetRequired)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetReadOnly)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetDisplayName)
				.Concat(AttributeTypes.PXUIFieldAttribute.SetNeutralDisplayName)
				.Concat(AttributeTypes.PXStringListAttribute.SetList)
				.Concat(AttributeTypes.PXStringListAttribute.AppendList)
				.Concat(AttributeTypes.PXStringListAttribute.SetLocalizable)
				.Concat(AttributeTypes.PXIntListAttribute.SetList)
				.ToImmutableHashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
		}
	}
}
