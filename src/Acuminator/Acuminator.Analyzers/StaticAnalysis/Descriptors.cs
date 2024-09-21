﻿
using System.Collections.Concurrent;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public enum Category
	{
		Acuminator
	}

	public static class Descriptors
	{
		private const string DocumentationLinkPrefix = @"https://github.com/Acumatica/Acuminator/blob/master/docs/diagnostics";
		private const string DocumentatonFileExtension = "md";

		private static readonly ConcurrentDictionary<Category, string> _categoryMapping = new ConcurrentDictionary<Category, string>();

		private static DiagnosticDescriptor Rule(string id, LocalizableString title, Category category, DiagnosticSeverity defaultSeverity,
												 string diagnosticShortName, LocalizableString? messageFormat = null, LocalizableString? description = null,
												 string? diagnosticDefaultJustification = null)
		{
			bool isEnabledByDefault = true;
			messageFormat = messageFormat ?? title;
			string diagnosticLink = $"{DocumentationLinkPrefix}/{id}.{DocumentatonFileExtension}";
			string[] customTags = diagnosticDefaultJustification.IsNullOrWhiteSpace()
				? [diagnosticShortName]
				: [diagnosticShortName, diagnosticDefaultJustification];

			return new DiagnosticDescriptor(id, title, messageFormat, _categoryMapping.GetOrAdd(category, c => c.ToString()), defaultSeverity,
											isEnabledByDefault, description, diagnosticLink, customTags);
		}

		public static DiagnosticDescriptor PX1000_InvalidPXActionHandlerSignature { get; } =
			Rule("PX1000", nameof(Resources.PX1000Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1000);

		public static DiagnosticDescriptor PX1001_PXGraphCreateInstance { get; } =
			Rule("PX1001", nameof(Resources.PX1001Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1001);

		public static DiagnosticDescriptor PX1002_MissingTypeListAttributeAnalyzer { get; } =
			Rule("PX1002", nameof(Resources.PX1002Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1002);

		public static DiagnosticDescriptor PX1003_NonSpecificPXGraphCreateInstance { get; } =
			Rule("PX1003", nameof(Resources.PX1003Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Warning, DiagnosticsShortName.PX1003);

		public static DiagnosticDescriptor PX1004_ViewDeclarationOrder { get; } =
			Rule("PX1004", nameof(Resources.PX1004Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Info, DiagnosticsShortName.PX1004);

		public static DiagnosticDescriptor PX1005_TypoInViewDelegateName { get; } =
			Rule("PX1005", nameof(Resources.PX1005ViewDelegateTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1005ViewDelegate, nameof(Resources.PX1005ViewDelegateMessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1005_TypoInActionDelegateName { get; } =
			Rule("PX1005", nameof(Resources.PX1005ActionDelegateTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1005ActionDelegate, nameof(Resources.PX1005ActionDelegateMessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1006_ViewDeclarationOrder { get; } =
			Rule("PX1006", nameof(Resources.PX1006Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Info, DiagnosticsShortName.PX1006);

		public static DiagnosticDescriptor PX1007_PublicClassNoXmlComment { get; } =
			Rule("PX1007", nameof(Resources.PX1007Title_MissingDescription).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Warning, DiagnosticsShortName.PX1007NoXmlComment,
				diagnosticDefaultJustification: DiagnosticsDefaultJustification.PX1007);

		public static DiagnosticDescriptor PX1007_InvalidProjectionDacFieldDescription { get; } =
			Rule("PX1007", nameof(Resources.PX1007Title_IncorrectProjectionDacFieldDescription).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1007IncorrectProjectionDacFieldComment,
				diagnosticDefaultJustification: DiagnosticsDefaultJustification.PX1007);

		public static DiagnosticDescriptor PX1008_LongOperationDelegateClosures { get; } =
			Rule("PX1008", nameof(Resources.PX1008Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1008);

		public static DiagnosticDescriptor PX1009_InheritanceFromPXCacheExtension { get; } =
			Rule("PX1009", nameof(Resources.PX1009Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1009);

		public static DiagnosticDescriptor PX1010_StartRowResetForPaging { get; } =
			Rule("PX1010", nameof(Resources.PX1010Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Warning, DiagnosticsShortName.PX1010);

		public static DiagnosticDescriptor PX1011_InheritanceFromPXCacheExtension { get; } =
			Rule("PX1011", nameof(Resources.PX1011Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Warning, DiagnosticsShortName.PX1011);

		public static DiagnosticDescriptor PX1012_PXActionOnNonPrimaryDac { get; } =
			Rule("PX1012", nameof(Resources.PX1012Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Warning, DiagnosticsShortName.PX1012);

		public static DiagnosticDescriptor PX1013_PXActionHandlerInvalidReturnType { get; } =
			Rule("PX1013", nameof(Resources.PX1013Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1013);

		public static DiagnosticDescriptor PX1014_NonNullableTypeForBqlField { get; } =
			Rule("PX1014", nameof(Resources.PX1014Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1014);

		public static DiagnosticDescriptor PX1015_PXBqlParametersMismatchWithOnlyRequiredParams { get; } =
			Rule("PX1015", nameof(Resources.PX1015Title).GetLocalized(),
				Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1015,
				nameof(Resources.PX1015MessageFormatWithOnlyRequiredParams).GetLocalized());

		public static DiagnosticDescriptor PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams { get; } =
			Rule("PX1015", nameof(Resources.PX1015Title).GetLocalized(),
				 Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1015,
				nameof(Resources.PX1015MessageFormatWithRequiredAndOptionalParams).GetLocalized());

		public static DiagnosticDescriptor PX1016_NoIsActiveMethodForDacExtension { get; } =
			Rule("PX1016", nameof(Resources.PX1016DacExtensionTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1016, diagnosticDefaultJustification: DiagnosticsDefaultJustification.PX1016);

		public static DiagnosticDescriptor PX1016_NoIsActiveMethodForGraphExtension { get; } =
			Rule("PX1016", nameof(Resources.PX1016GraphExtensionTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1016, diagnosticDefaultJustification: DiagnosticsDefaultJustification.PX1016);

		public static DiagnosticDescriptor PX1018_NoPrimaryViewForPrimaryDac { get; } =
			Rule("PX1018", nameof(Resources.PX1018Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1018);

		public static DiagnosticDescriptor PX1019_AutoNumberOnDacPropertyWithNonStringType { get; } =
			Rule("PX1019", nameof(Resources.PX1019Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1019);

		public static DiagnosticDescriptor PX1020_InsufficientStringLengthForDacPropertyWithAutoNumbering { get; } =
			Rule("PX1020", nameof(Resources.PX1020Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1020);

		public static DiagnosticDescriptor PX1021_PXDBFieldAttributeNotMatchingDacProperty { get; } =
			Rule("PX1021", nameof(Resources.PX1021Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1021);

		public static DiagnosticDescriptor PX1022_NonPublicDac { get; } =
			Rule("PX1022", nameof(Resources.PX1022DacTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1022Dac);

		public static DiagnosticDescriptor PX1022_NonPublicDacExtension { get; } =
			Rule("PX1022", nameof(Resources.PX1022DacExtensionTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1022DacExtension);

		public static DiagnosticDescriptor PX1022_NonPublicGraph { get; } =
			Rule("PX1022", nameof(Resources.PX1022GraphTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1022Graph);

		public static DiagnosticDescriptor PX1022_NonPublicGraphExtension { get; } =
			Rule("PX1022", nameof(Resources.PX1022GraphExtensionTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				 DiagnosticsShortName.PX1022GraphExtension);

		public static DiagnosticDescriptor PX1023_MultipleTypeAttributesOnProperty { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleTypeAttributesOnPropertyTitle).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1023);

		public static DiagnosticDescriptor PX1023_MultipleTypeAttributesOnAggregators { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleTypeAttributesOnAggregatorsTitle).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1023);

		public static DiagnosticDescriptor PX1023_MultipleCalcedOnDbSideAttributesOnProperty { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleCalcedOnDbSideAttributesOnPropertyTitle).GetLocalized(),
				 Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1023);

		public static DiagnosticDescriptor PX1023_MultipleCalcedOnDbSideAttributesOnAggregators { get; } =
			Rule("PX1023", nameof(Resources.PX1023MultipleCalcedOnDbSideAttributesOnAggregatorsTitle).GetLocalized(),
				 Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1023);

		public static DiagnosticDescriptor PX1024_DacNonAbstractFieldType { get; } =
			Rule("PX1024", nameof(Resources.PX1024Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1024);

		public static DiagnosticDescriptor PX1026_UnderscoresInDacDeclaration { get; } =
			Rule("PX1026", nameof(Resources.PX1026Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1026);

		public static DiagnosticDescriptor PX1027_ForbiddenFieldsInDacDeclaration { get; } =
			Rule("PX1027", nameof(Resources.PX1027ForbiddenFieldsTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1027ForbiddenFieldsInDac, nameof(Resources.PX1027ForbiddenFieldsMessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1027_ForbiddenFieldsInDacDeclaration_NonISV { get; } =
			Rule("PX1027", nameof(Resources.PX1027ForbiddenFieldsTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1027ForbiddenFieldsInDac, nameof(Resources.PX1027ForbiddenFieldsMessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1027_ForbiddenCompanyPrefixInDacFieldName { get; } =
			Rule("PX1027", nameof(Resources.PX1027ForbiddenCompanyPrefixInDacFieldTitle).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1027ForbiddenCompanyPrefixInDacField);

		public static DiagnosticDescriptor PX1028_ConstructorInDacDeclaration { get; } =
			Rule("PX1028", nameof(Resources.PX1028Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1028);

		public static DiagnosticDescriptor PX1029_PXGraphUsageInDac { get; } =
			Rule("PX1029", nameof(Resources.PX1029Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1029);

		public static DiagnosticDescriptor PX1030_DefaultAttibuteToExistingRecordsError { get; } =
			Rule("PX1030", nameof(Resources.PX1030TitleDefaultAttributeOnDacExtension).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1030);

		public static DiagnosticDescriptor PX1030_DefaultAttibuteToExistingRecordsWarning { get; } =
			Rule("PX1030", nameof(Resources.PX1030TitleDefaultAttributeOnDacExtension).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Warning, DiagnosticsShortName.PX1030);

		public static DiagnosticDescriptor PX1030_DefaultAttibuteToExistingRecordsOnDAC { get; } =
			Rule("PX1030", nameof(Resources.PX1030TitleDefaultAttributeOnDac).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1030);

		public static DiagnosticDescriptor PX1031_DacCannotContainInstanceMethods { get; } =
			Rule("PX1031", nameof(Resources.PX1031Title).GetLocalized(), Category.Acuminator,
				DiagnosticSeverity.Error, DiagnosticsShortName.PX1031);

		public static DiagnosticDescriptor PX1032_DacPropertyCannotContainMethodInvocations { get; } =
			Rule("PX1032", nameof(Resources.PX1032Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1032);

		public static DiagnosticDescriptor PX1033_MissingDacPrimaryKeyDeclaration { get; } =
			Rule("PX1033", nameof(Resources.PX1033Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Info,
				DiagnosticsShortName.PX1033);

		public static DiagnosticDescriptor PX1034_MissingDacForeignKeyDeclaration { get; } =
			Rule("PX1034", nameof(Resources.PX1034Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Info,
				DiagnosticsShortName.PX1034);

		public static DiagnosticDescriptor PX1035_MultipleKeyDeclarationsInDacWithSameFields { get; } =
			Rule("PX1035", nameof(Resources.PX1035Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1035);

		public static DiagnosticDescriptor PX1036_WrongDacPrimaryKeyName { get; } =
			Rule("PX1036", nameof(Resources.PX1036PKTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1036PK);

		public static DiagnosticDescriptor PX1036_WrongDacSingleUniqueKeyName { get; } =
			Rule("PX1036", nameof(Resources.PX1036SingleUKTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1036SingleUK);

		public static DiagnosticDescriptor PX1036_WrongDacMultipleUniqueKeyDeclarations { get; } =
			Rule("PX1036", nameof(Resources.PX1036MultipleUKTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1036MultipleUK);

		public static DiagnosticDescriptor PX1036_WrongDacForeignKeyDeclaration { get; } =
			Rule("PX1036", nameof(Resources.PX1036FKTitle).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1036FK);

		public static DiagnosticDescriptor PX1037_UnboundDacFieldInKeyDeclaration { get; } =
			Rule("PX1037", nameof(Resources.PX1037Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1037);

		public static DiagnosticDescriptor PX1040_ConstructorInGraphExtension { get; } =
			Rule("PX1040", nameof(Resources.PX1040Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1040);

		public static DiagnosticDescriptor PX1041_NameConventionEventsInGraphsAndGraphExtensions { get; } =
			Rule("PX1041", nameof(Resources.PX1041Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Info,
				DiagnosticsShortName.PX1041);

		public static DiagnosticDescriptor PX1042_DatabaseQueriesInRowSelecting { get; } =
			Rule("PX1042", nameof(Resources.PX1042Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1042);

		public static DiagnosticDescriptor PX1043_SavingChangesInEventHandlers { get; } =
			Rule("PX1043", nameof(Resources.PX1043Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1043);

		public static DiagnosticDescriptor PX1043_SavingChangesInRowPerstisting { get; } =
			Rule("PX1043", nameof(Resources.PX1043TitleRowPersisting).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1043);

		public static DiagnosticDescriptor PX1043_SavingChangesInRowPerstistedNonISV { get; } =
			Rule("PX1043", nameof(Resources.PX1043TitleRowPersistedNonISV).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1043);

		public static DiagnosticDescriptor PX1044_ChangesInPXCacheInEventHandlers { get; } =
			Rule("PX1044", nameof(Resources.PX1044Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1044, nameof(Resources.PX1044MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1045_PXGraphCreateInstanceInEventHandlers { get; } =
			Rule("PX1045", nameof(Resources.PX1045Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1045);

		public static DiagnosticDescriptor PX1045_PXGraphCreateInstanceInEventHandlers_NonISV { get; } =
			Rule("PX1045", nameof(Resources.PX1045Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1045);

		public static DiagnosticDescriptor PX1046_LongOperationInEventHandlers { get; } =
			Rule("PX1046", nameof(Resources.PX1046Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1046);

		public static DiagnosticDescriptor PX1047_RowChangesInEventHandlersForbiddenForArgs { get; } =
			Rule("PX1047", nameof(Resources.PX1047Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1047, nameof(Resources.PX1047MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1047_RowChangesInEventHandlersForbiddenForArgs_NonISV { get; } =
			Rule("PX1047", nameof(Resources.PX1047Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1047, nameof(Resources.PX1047MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1048_RowChangesInEventHandlersAllowedForArgsOnly { get; } =
			Rule("PX1048", nameof(Resources.PX1048Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1048, nameof(Resources.PX1048MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1049_DatabaseQueriesInRowSelected { get; } =
			Rule("PX1049", nameof(Resources.PX1049Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1049);

		public static DiagnosticDescriptor PX1050_HardcodedStringInLocalizationMethod { get; } =
			Rule("PX1050", nameof(Resources.PX1050Title_HardcodedString).GetLocalized(),
				 Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1050);

		public static DiagnosticDescriptor PX1050_NonConstFieldStringInLocalizationMethod { get; } =
			Rule("PX1050", nameof(Resources.PX1050Title_NonConstFieldString).GetLocalized(),
				 Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1050);

		public static DiagnosticDescriptor PX1051_NonLocalizableString { get; } =
			Rule("PX1051", nameof(Resources.PX1051Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1051);

		public static DiagnosticDescriptor PX1052_IncorrectStringToFormat { get; } =
			Rule("PX1052", nameof(Resources.PX1052Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1052);

		public static DiagnosticDescriptor PX1053_ConcatenationPriorLocalization { get; } =
			Rule("PX1053", nameof(Resources.PX1053Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1053);

		public static DiagnosticDescriptor PX1054_PXGraphLongRunOperationDuringInitialization { get; } =
			Rule("PX1054", nameof(Resources.PX1054Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1054);

		public static DiagnosticDescriptor PX1055_DacKeyFieldsWithIdentityKeyField { get; } =
			Rule("PX1055", nameof(Resources.PX1055Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1055);

		public static DiagnosticDescriptor PX1056_PXGraphCreationInIsActiveMethod { get; } =
			Rule("PX1056", nameof(Resources.PX1056TitleIsActive).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1056IsActiveMethod);

		public static DiagnosticDescriptor PX1056_PXGraphCreationInIsActiveForGraphMethod { get; } =
			Rule("PX1056", nameof(Resources.PX1056TitleIsActiveForGraph).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error,
				DiagnosticsShortName.PX1056IsActiveForGraphMethod);

		public static DiagnosticDescriptor PX1057_PXGraphCreationDuringInitialization { get; } =
			Rule("PX1057", nameof(Resources.PX1057Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1057);

		public static DiagnosticDescriptor PX1057_PXGraphCreationDuringInitialization_NonISV { get; } =
			Rule("PX1057", nameof(Resources.PX1057Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1057);

		public static DiagnosticDescriptor PX1058_PXGraphSavingChangesDuringInitialization { get; } =
			Rule("PX1058", nameof(Resources.PX1058Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1058);

		public static DiagnosticDescriptor PX1059_ChangesInPXCacheDuringPXGraphInitialization { get; } =
			Rule("PX1059", nameof(Resources.PX1059Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1059);

		public static DiagnosticDescriptor PX1060_LegacyBqlField { get; } =
			Rule("PX1060", nameof(Resources.PX1060Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Info, DiagnosticsShortName.PX1060);

		public static DiagnosticDescriptor PX1061_LegacyBqlConstant { get; } =
			Rule("PX1061", nameof(Resources.PX1061Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Info, DiagnosticsShortName.PX1061);

		public static DiagnosticDescriptor PX1062_StaticFieldOrPropertyInGraph { get; } =
			Rule("PX1062", nameof(Resources.PX1062MessageFormat).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1062);

		public static DiagnosticDescriptor PX1063_NoSerializationConstructorInException { get; } =
			Rule("PX1063", nameof(Resources.PX1063Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1063);

		public static DiagnosticDescriptor PX1064_NoGetObjectDataOverrideInExceptionWithNewFields { get; } =
			Rule("PX1064", nameof(Resources.PX1064Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1064);

		public static DiagnosticDescriptor PX1065_NoBqlFieldForDacFieldProperty { get; } =
			Rule("PX1065", nameof(Resources.PX1065TitleFormat).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1065);

		public static DiagnosticDescriptor PX1066_TypoInBqlFieldName { get; } =
			Rule("PX1066", nameof(Resources.PX1066TitleFormat).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1066);

		public static DiagnosticDescriptor PX1067_MissingBqlFieldRedeclarationInDerivedDac { get; } =
			Rule("PX1067", nameof(Resources.PX1067TitleFormat).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1067);

		public static DiagnosticDescriptor PX1068_PropertyAndBqlFieldTypesMismatch { get; } =
			Rule("PX1068", nameof(Resources.PX1068Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1068);

		public static DiagnosticDescriptor PX1070_UiPresentationLogicInEventHandlers { get; } =
			Rule("PX1070", nameof(Resources.PX1070Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1070);

		public static DiagnosticDescriptor PX1071_PXActionExecutionInEventHandlers { get; } =
			Rule("PX1071", nameof(Resources.PX1071Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1071);

		public static DiagnosticDescriptor PX1071_PXActionExecutionInEventHandlers_NonISV { get; } =
			Rule("PX1071", nameof(Resources.PX1071Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1071);

		public static DiagnosticDescriptor PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable { get; } =
			Rule("PX1072", nameof(Resources.PX1072Title_ReuseExistingGraphVariable).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1072);

		public static DiagnosticDescriptor PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable { get; } =
			Rule("PX1072", nameof(Resources.PX1072Title_CreateSharedGraphVariable).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1072);

		public static DiagnosticDescriptor PX1073_ThrowingExceptionsInRowPersisted { get; } =
			Rule("PX1073", nameof(Resources.PX1073Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1073);

		public static DiagnosticDescriptor PX1073_ThrowingExceptionsInRowPersisted_NonISV { get; } =
			Rule("PX1073", nameof(Resources.PX1073Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1073);

		public static DiagnosticDescriptor PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers { get; } =
			Rule("PX1074", nameof(Resources.PX1074Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning,
				DiagnosticsShortName.PX1074, nameof(Resources.PX1074MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1075_RaiseExceptionHandlingInEventHandlers { get; } =
			Rule("PX1075", nameof(Resources.PX1075Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1075,
				nameof(Resources.PX1075MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1075_RaiseExceptionHandlingInEventHandlers_NonISV { get; } =
			Rule("PX1075", nameof(Resources.PX1075Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1075,
				nameof(Resources.PX1075MessageFormat).GetLocalized());

		public static DiagnosticDescriptor PX1076_CallToPXInternalUseOnlyAPI_OnlyISV { get; } =
			Rule("PX1076", nameof(Resources.PX1076Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1076);

		public static DiagnosticDescriptor PX1077_EventHandlersShouldBeProtectedVirtual { get; } =
			Rule("PX1077", nameof(Resources.PX1077Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1077,
				messageFormat: nameof(Resources.PX1077TitleFormatWithReason).GetLocalized());

		public static DiagnosticDescriptor PX1077_EventHandlersShouldNotBePrivate { get; } =
			Rule("PX1077", nameof(Resources.PX1077Title_EventHandlersShouldNotBePrivate).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1077);

		public static DiagnosticDescriptor PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations { get; } =
			Rule("PX1077", nameof(Resources.PX1077Title_ExplicitInterfaceImplementation).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1077);

		public static DiagnosticDescriptor PX1080_DataViewDelegateLongOperationStart { get; } =
			Rule("PX1080", nameof(Resources.PX1080Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1080);

		public static DiagnosticDescriptor PX1081_PXGraphExecutesActionDuringInitialization { get; } =
			Rule("PX1081", nameof(Resources.PX1081Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1081);

		public static DiagnosticDescriptor PX1082_ActionExecutionInDataViewDelegate { get; } =
			Rule("PX1082", nameof(Resources.PX1082Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1082);

		public static DiagnosticDescriptor PX1083_SavingChangesInDataViewDelegate { get; } =
			Rule("PX1083", nameof(Resources.PX1083Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1083);

		public static DiagnosticDescriptor PX1084_GraphCreationInDataViewDelegate { get; } =
			Rule("PX1084", nameof(Resources.PX1084Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1084);

		public static DiagnosticDescriptor PX1085_DatabaseQueriesInPXGraphInitialization { get; } =
			Rule("PX1085", nameof(Resources.PX1085Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1085);

		public static DiagnosticDescriptor PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation { get; } =
			Rule("PX1086", nameof(Resources.PX1086Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1086);

		public static DiagnosticDescriptor PX1087_CausingStackOverflowExceptionInBaseViewDelegateInvocation { get; } =
			Rule("PX1087", nameof(Resources.PX1087Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1087);

		public static DiagnosticDescriptor PX1089_UiPresentationLogicInActionDelegates { get; } =
			Rule("PX1089", nameof(Resources.PX1089Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1089);

		public static DiagnosticDescriptor PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers { get; } =
			Rule("PX1090", nameof(Resources.PX1090Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1090);

		public static DiagnosticDescriptor PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation { get; } =
			Rule("PX1091", nameof(Resources.PX1091Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1091);

		public static DiagnosticDescriptor PX1092_MissingAttributesOnActionHandler { get; } =
			Rule("PX1092", nameof(Resources.PX1092Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1092);

		public static DiagnosticDescriptor PX1093_GraphDeclarationViolation { get; } =
			Rule("PX1093", nameof(Resources.PX1093Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1093);

		public static DiagnosticDescriptor PX1094_DacShouldHaveUiAttribute { get; } =
			Rule("PX1094", nameof(Resources.PX1094Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1094);

		public static DiagnosticDescriptor PX1095_PXDBCalcedMustBeAccompaniedNonDBTypeAttribute { get; } =
			Rule("PX1095", nameof(Resources.PX1095Title_PXDBCalced).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1095PXDBCalced);

		public static DiagnosticDescriptor PX1095_PXDBScalarMustBeAccompaniedNonDBTypeAttribute { get; } =
			Rule("PX1095", nameof(Resources.PX1095Title_PXDBScalar).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1095PXDBScalar);

		public static DiagnosticDescriptor PX1096_PXOverrideMustMatchSignature { get; } =
			Rule("PX1096", nameof(Resources.PX1096Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Error, DiagnosticsShortName.PX1096);

		public static DiagnosticDescriptor PX1099_ForbiddenApiUsage_WithoutReason { get; } =
			Rule("PX1099", nameof(Resources.PX1099Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1099,
				 messageFormat: nameof(Resources.PX1099TitleFormat).GetLocalized());

		public static DiagnosticDescriptor PX1099_ForbiddenApiUsage_WithReason { get; } =
			Rule("PX1099", nameof(Resources.PX1099Title).GetLocalized(), Category.Acuminator, DiagnosticSeverity.Warning, DiagnosticsShortName.PX1099,
				 messageFormat: nameof(Resources.PX1099TitleFormatWithReason).GetLocalized());
	}
}
