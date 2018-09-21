using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public enum Category
	{
		Default,
	}

	public static class Descriptors
	{
		private static readonly ConcurrentDictionary<Category, string> categoryMapping = new ConcurrentDictionary<Category, string>();

        private static DiagnosticDescriptor Rule(string id, LocalizableString title, Category category, DiagnosticSeverity defaultSeverity, 
										 LocalizableString messageFormat = null, LocalizableString description = null)
		{
			bool isEnabledByDefault = true;
			messageFormat = messageFormat ?? title;
			return new DiagnosticDescriptor(id, title, messageFormat, categoryMapping.GetOrAdd(category, c => c.ToString()), defaultSeverity,
											isEnabledByDefault, description);
		}

		public static DiagnosticDescriptor PX1000_InvalidPXActionHandlerSignature { get; } = 
            Rule("PX1000", nameof(Resources.PX1000Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1001_PXGraphCreateInstance { get; } = 
            Rule("PX1001", nameof(Resources.PX1001Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1002_MissingTypeListAttributeAnalyzer { get; } = 
            Rule("PX1002", nameof(Resources.PX1002Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1003_NonSpecificPXGraphCreateInstance { get; } = 
            Rule("PX1003", nameof(Resources.PX1003Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

        public static DiagnosticDescriptor PX1004_ViewDeclarationOrder { get; } = 
            Rule("PX1004", nameof(Resources.PX1004Title).GetLocalized(), Category.Default, DiagnosticSeverity.Info);

		public static DiagnosticDescriptor PX1005_TypoInViewDelegateName { get; } = 
            Rule("PX1005", nameof(Resources.PX1005Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning, 
                nameof(Resources.PX1005MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1006_ViewDeclarationOrder { get; } = 
            Rule("PX1006", nameof(Resources.PX1006Title).GetLocalized(), Category.Default, DiagnosticSeverity.Info);

        public static DiagnosticDescriptor PX1008_LongOperationDelegateClosures { get; } = 
            Rule("PX1008", nameof(Resources.PX1008Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

        public static DiagnosticDescriptor PX1010_StartRowResetForPaging { get; } = 
            Rule("PX1010", nameof(Resources.PX1010Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		public static DiagnosticDescriptor PX1009_InheritanceFromPXCacheExtension { get; } = 
            Rule("PX1009", nameof(Resources.PX1009Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1011_InheritanceFromPXCacheExtension { get; } =
            Rule("PX1011", nameof(Resources.PX1011Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		public static DiagnosticDescriptor PX1012_PXActionOnNonPrimaryView { get; } =
			Rule("PX1012", nameof(Resources.PX1012Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		public static DiagnosticDescriptor PX1014_NonNullableTypeForBqlField { get; } = 
            Rule("PX1014", nameof(Resources.PX1014Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1015_PXBqlParametersMismatchWithOnlyRequiredParams { get; } = 
            Rule("PX1015", nameof(Resources.PX1015Title).GetLocalized(), 
                Category.Default, DiagnosticSeverity.Warning,
	            nameof(Resources.PX1015MessageFormatWithOnlyRequiredParams).GetLocalized());

        public static DiagnosticDescriptor PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams { get; } =
            Rule("PX1015", nameof(Resources.PX1015Title).GetLocalized(), 
                 Category.Default, DiagnosticSeverity.Warning,
	            nameof(Resources.PX1015MessageFormatWithRequiredAndOptionalParams).GetLocalized());

		public static DiagnosticDescriptor PX1018_NoPrimaryViewForPrimaryDac { get; } =
			Rule("PX1018", nameof(Resources.PX1018Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1021_PXDBFieldAttributeNotMatchingDacProperty { get; } =
            Rule("PX1021", nameof(Resources.PX1021Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1023_MultipleTypeAttributesOnProperty { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleTypeAttributesOnPropertyTitle).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1023_MultipleTypeAttributesOnAggregators { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleTypeAttributesOnAggregatorsTitle).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1023_MultipleSpecialTypeAttributesOnProperty { get; } =
            Rule("PX1023", nameof(Resources.PX1023MultipleSpecialTypeAttributesOnPropertyTitle).GetLocalized(),
                 Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1023_MultipleSpecialTypeAttributesOnAggregators { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleSpecialTypeAttributesOnAggregatorsTitle).GetLocalized(),
				 Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1024_DacNonAbstractFieldType { get; } =
			Rule("PX1024", nameof(Resources.PX1024Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1026_UnderscoresInDacDeclaration { get; } =
			Rule("PX1026", nameof(Resources.PX1026Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1027_ForbiddenFieldsInDacDeclaration { get; } =
			Rule("PX1027", nameof(Resources.PX1027Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error,
				nameof(Resources.PX1027MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1028_ConstructorInDacDeclaration { get; } =
			Rule("PX1028", nameof(Resources.PX1028Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1029_PXGraphUsageInDac { get; } =
			Rule("PX1029", nameof(Resources.PX1029Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1030_DefaultAttibuteToExisitingRecords { get; } =
			Rule("PX1030", nameof(Resources.PX1030Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		public static DiagnosticDescriptor PX1031_DacCannotContainInstanceMethods { get; } =
			Rule("PX1031", nameof(Resources.PX1031Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1032_DacPropertyCannotContainMethodInvocations { get; } =
			Rule("PX1032", nameof(Resources.PX1032Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1040_ConstructorInGraphExtension { get; } =
			Rule("PX1040", nameof(Resources.PX1040Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1042_DatabaseQueriesInRowSelecting { get; } =
			Rule("PX1042", nameof(Resources.PX1042Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1043_SavingChangesInEventHandlers { get; } =
			Rule("PX1043", nameof(Resources.PX1043Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1043_SavingChangesInRowPerstisting { get; } =
			Rule("PX1043", nameof(Resources.PX1043TitleRowPersisting).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1044_ChangesInPXCacheInEventHandlers { get; } =
			Rule("PX1044", nameof(Resources.PX1044Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error,
				nameof(Resources.PX1044MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1045_PXGraphCreateInstanceInEventHandlers { get; } =
			Rule("PX1045", nameof(Resources.PX1045Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1046_LongOperationInEventHandlers { get; } =
			Rule("PX1046", nameof(Resources.PX1046Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

		public static DiagnosticDescriptor PX1047_RowChangesInEventHandlersForbiddenForArgs { get; } =
			Rule("PX1047", nameof(Resources.PX1047Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error,
				nameof(Resources.PX1047MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1048_RowChangesInEventHandlersAllowedForArgsOnly { get; } =
			Rule("PX1048", nameof(Resources.PX1048Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error,
				nameof(Resources.PX1048MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1049_DatabaseQueriesInRowSelected { get; } =
			Rule("PX1049", nameof(Resources.PX1049Title).GetLocalized(), Category.Default, DiagnosticSeverity.Warning);

		public static DiagnosticDescriptor PX1050_HardcodedStringInLocalizationMethod { get; } =
            Rule("PX1050", nameof(Resources.PX1050Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1051_NonLocalizableString { get; } =
            Rule("PX1051", nameof(Resources.PX1051Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1052_IncorrectStringToFormat { get; } =
            Rule("PX1052", nameof(Resources.PX1052Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1053_ConcatenationPriorLocalization { get; } =
            Rule("PX1053", nameof(Resources.PX1053Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1054_PXGraphLongRunOperationDuringInitialization { get; } =
            Rule("PX1054", nameof(Resources.PX1054Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1057_PXGraphCreationDuringInitialization { get; } =
            Rule("PX1057", nameof(Resources.PX1057Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1058_PXGraphSavingChangesDuringInitialization { get; } =
            Rule("PX1058", nameof(Resources.PX1058Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);

        public static DiagnosticDescriptor PX1059_PXGraphChangesPXCacheDuringInitialization { get; } =
            Rule("PX1059", nameof(Resources.PX1059Title).GetLocalized(), Category.Default, DiagnosticSeverity.Error);
    }
}
