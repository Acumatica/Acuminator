using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public class PXContext
	{
		private const string PXSelectBase_Acumatica2018R2 = "PX.Data.PXSelectBase`2";
		private const string IViewConfig_Acumatica2018R2 = "PX.Data.PXSelectBase`2+IViewConfig";

		public bool IsAcumatica2018R2 { get; }

		public Compilation Compilation { get; }

		private readonly Lazy<BQLSymbols> _bql;
		public BQLSymbols BQL => _bql.Value;

		private readonly Lazy<EventSymbols> _events;
		public EventSymbols Events => _events.Value;

		private readonly Lazy<FieldAttributeSymbols> _fieldAttributes;
		public FieldAttributeSymbols FieldAttributes => _fieldAttributes.Value;

		private readonly Lazy<PXSystemActionSymbols> _systemActionTypes;
		public PXSystemActionSymbols PXSystemActions => _systemActionTypes.Value;

		private readonly Lazy<SystemTypeSymbols> _systemTypes;
		public SystemTypeSymbols SystemTypes => _systemTypes.Value;
		private readonly Lazy<AttributeSymbols> _attributes;
		public AttributeSymbols AttributeTypes => _attributes.Value;

        private readonly Lazy<LocalizationSymbols> _localizationMethods;
        public LocalizationSymbols Localization => _localizationMethods.Value;

        private readonly Lazy<PXGraphRelatedMethodSymbols> _pxGraphRelatedMethods;
        public PXGraphRelatedMethodSymbols PXGraphRelatedMethods => _pxGraphRelatedMethods.Value;

        private readonly Lazy<PXCacheSymbols> _pxCache;
        public PXCacheSymbols PXCache => _pxCache.Value;

		private readonly Lazy<PXActionSymbols> _pxAction;
		public PXActionSymbols PXAction => _pxAction.Value;

        private readonly Lazy<PXSelectBaseGenericSymbols> _pxSelectBaseGeneric;
        public PXSelectBaseGenericSymbols PXSelectBaseGeneric => _pxSelectBaseGeneric.Value;

		private readonly Lazy<ImmutableHashSet<IMethodSymbol>> _uiPresentationLogicMethods;
		public ImmutableHashSet<IMethodSymbol> UiPresentationLogicMethods => _uiPresentationLogicMethods.Value;

        public INamedTypeSymbol PXGraphType => Compilation.GetTypeByMetadataName(typeof(PX.Data.PXGraph).FullName);
		public INamedTypeSymbol PXProcessingBaseType => Compilation.GetTypeByMetadataName(typeof(PXProcessingBase<>).FullName);
		public INamedTypeSymbol PXGraphExtensionType => Compilation.GetTypeByMetadataName(typeof(PXGraphExtension).FullName);
		public INamedTypeSymbol PXCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXCacheExtension).FullName);
		public INamedTypeSymbol PXMappedCacheExtensionType => Compilation.GetTypeByMetadataName(typeof(PXMappedCacheExtension).FullName);
		public INamedTypeSymbol PXViewType => Compilation.GetTypeByMetadataName(typeof(PXView).FullName);
		public INamedTypeSymbol PXSelectBaseType => Compilation.GetTypeByMetadataName(typeof(PXSelectBase).FullName);
		public INamedTypeSymbol PXLongOperation => Compilation.GetTypeByMetadataName(typeof(PXLongOperation).FullName);

		public INamedTypeSymbol PXSelectBase2018R2NewType => Compilation.GetTypeByMetadataName(PXSelectBase_Acumatica2018R2);
		public INamedTypeSymbol IViewConfig2018R2 => Compilation.GetTypeByMetadataName(IViewConfig_Acumatica2018R2);

		public INamedTypeSymbol PXActionCollection => Compilation.GetTypeByMetadataName(typeof(PXActionCollection).FullName);

		public INamedTypeSymbol PXAdapterType => Compilation.GetTypeByMetadataName(typeof(PXAdapter).FullName);
		public INamedTypeSymbol IBqlTableType => Compilation.GetTypeByMetadataName(typeof(IBqlTable).FullName);
		public INamedTypeSymbol IBqlFieldType => Compilation.GetTypeByMetadataName(typeof(IBqlField).FullName);

		public INamedTypeSymbol IPXResultsetType => Compilation.GetTypeByMetadataName(typeof(IPXResultset).FullName);
		public INamedTypeSymbol PXResult => Compilation.GetTypeByMetadataName(typeof(PXResult).FullName);

		public INamedTypeSymbol PXFieldState => Compilation.GetTypeByMetadataName(typeof(PXFieldState).FullName);
		public INamedTypeSymbol PXAttributeFamily => Compilation.GetTypeByMetadataName(typeof(PXAttributeFamilyAttribute).FullName);

        public INamedTypeSymbol PXException => Compilation.GetTypeByMetadataName(typeof(PXException).FullName);
        public INamedTypeSymbol PXBaseRedirectException => Compilation.GetTypeByMetadataName(typeof(PXBaseRedirectException).FullName);

        public INamedTypeSymbol IPXLocalizableList => Compilation.GetTypeByMetadataName(typeof(IPXLocalizableList).FullName);
		public INamedTypeSymbol PXConnectionScope => Compilation.GetTypeByMetadataName(typeof(PXConnectionScope).FullName);
		public INamedTypeSymbol PXDatabase => Compilation.GetTypeByMetadataName(typeof(PXDatabase).FullName);
		public INamedTypeSymbol PXSelectorAttribute => Compilation.GetTypeByMetadataName(typeof(PXSelectorAttribute).FullName);

        public INamedTypeSymbol InstanceCreatedEvents => Compilation.GetTypeByMetadataName(typeof(PX.Data.PXGraph.InstanceCreatedEvents).FullName);

        public ImmutableArray<ISymbol> StringFormat => SystemTypes.String.GetMembers(nameof(string.Format));
        public ImmutableArray<ISymbol> StringConcat => SystemTypes.String.GetMembers(nameof(string.Concat));
        public IMethodSymbol PXGraphExtensionInitializeMethod => PXGraphExtensionType.GetMembers(nameof(PXGraphExtension.Initialize))
                                                                 .OfType<IMethodSymbol>()
                                                                 .First();
        public ImmutableArray<IMethodSymbol> StartOperation => PXLongOperation.GetMembers(nameof(PX.Data.PXLongOperation.StartOperation))
                                                               .OfType<IMethodSymbol>()
                                                               .ToImmutableArray();

        public PXContext(Compilation compilation)
		{
			Compilation = compilation;

			_bql = new Lazy<BQLSymbols>(() => new BQLSymbols(Compilation));
			_events = new Lazy<EventSymbols>(() => new EventSymbols(Compilation));
			_fieldAttributes = new Lazy<FieldAttributeSymbols>(() => new FieldAttributeSymbols(Compilation));
			_systemActionTypes = new Lazy<PXSystemActionSymbols>(() => new PXSystemActionSymbols(Compilation));
			_attributes = new Lazy<AttributeSymbols>(() => new AttributeSymbols(Compilation));
			_systemTypes = new Lazy<SystemTypeSymbols>(() => new SystemTypeSymbols(Compilation));
            _localizationMethods = new Lazy<LocalizationSymbols>(() => new LocalizationSymbols(Compilation));
            _pxGraphRelatedMethods = new Lazy<PXGraphRelatedMethodSymbols>(() => new PXGraphRelatedMethodSymbols(this));
            _pxCache = new Lazy<PXCacheSymbols>(() => new PXCacheSymbols(Compilation));
			_pxAction = new Lazy<PXActionSymbols>(() => new PXActionSymbols(Compilation));
            _pxSelectBaseGeneric = new Lazy<PXSelectBaseGenericSymbols>(() => new PXSelectBaseGenericSymbols(Compilation));

			_uiPresentationLogicMethods = new Lazy<ImmutableHashSet<IMethodSymbol>>(GetUiPresentationLogicMethods);

            IsAcumatica2018R2 = PXSelectBase2018R2NewType != null;
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
				.ToImmutableHashSet();
		}
	}
}
