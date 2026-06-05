#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Acuminator.Utilities.Roslyn;
using Acuminator.Vsix.Utilities;

using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Coloriser
{
	[ContentType(Constants.CSharp.LegacyLanguageName)]
	[TagType(typeof(IClassificationTag))]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	[Export(typeof(IViewTaggerProvider))]
	public class PXColorizerTaggerProvider : IViewTaggerProvider
	{
		private readonly IClassificationTypeRegistryService _classificationRegistry;
		private readonly IClassificationFormatMapService _classificationFormatMapService;

		internal ITextDocumentFactoryService TextDocumentFactory { get; }

		private const string TextCategory = "text";

		private static readonly object _syncRoot = new object();
		private static volatile bool _isPriorityIncreased;

		private readonly Dictionary<PXCodeType, IClassificationType> _codeColoringClassificationTypes;

		public IClassificationType? this[PXCodeType codeType] =>
			_codeColoringClassificationTypes.TryGetValue(codeType, out IClassificationType type)
				 ? type
				 : null;

		private readonly Dictionary<int, IClassificationType> _braceTypeByLevel;

		public IClassificationType? this[int braceLevel] =>
			_braceTypeByLevel.TryGetValue(braceLevel, out IClassificationType type)
				 ? type
				 : null;

		[ImportingConstructor]
		public PXColorizerTaggerProvider(IClassificationTypeRegistryService classificationRegistry,
										 IClassificationFormatMapService classificationFormatMapService,
										 ITextDocumentFactoryService textDocumentFactory)
		{
			_classificationRegistry 		= classificationRegistry;
			_classificationFormatMapService = classificationFormatMapService;
			TextDocumentFactory 			= textDocumentFactory;

			_codeColoringClassificationTypes = GetClassificationTypesForAcumaticaCodeElements(_classificationRegistry);
			_braceTypeByLevel = GetClassificationTypesForAngleBraces(_classificationRegistry);

			IncreaseCommentFormatTypesPriority(_classificationRegistry, _classificationFormatMapService,
												_codeColoringClassificationTypes[PXCodeType.BqlParameter]);
		}

		public virtual ITagger<T>? CreateTagger<T>(ITextView textView, ITextBuffer textBuffer)
		where T : ITag
		{
			if (textView == null || textBuffer == null || textView.TextBuffer != textBuffer || !ThreadHelper.CheckAccess())
				return null;

			var tagger = textBuffer.Properties.GetOrCreateSingletonProperty(typeof(PXRoslynColorizerTagger), () =>
			{
				return new PXRoslynColorizerTagger(textBuffer, this, subscribeToSettingsChanges: true, useCacheChecking: true);
			});

			return tagger as ITagger<T>;
		}

		private static Dictionary<PXCodeType, IClassificationType> GetClassificationTypesForAcumaticaCodeElements(
																					IClassificationTypeRegistryService classificationRegistry)
		{
			IClassificationType bqlClassificationType = classificationRegistry.GetClassificationType(ColoringConstants.BQLOperatorFormat);
			var acumaticaCodeElementsClassificationTypes = new Dictionary<PXCodeType, IClassificationType>
			{
				[PXCodeType.Dac] 		  = classificationRegistry.GetClassificationType(ColoringConstants.DacFormat),
				[PXCodeType.DacExtension] = classificationRegistry.GetClassificationType(ColoringConstants.DacExtensionFormat),
				[PXCodeType.DacField] 	  = classificationRegistry.GetClassificationType(ColoringConstants.DacFieldFormat),
				[PXCodeType.BqlParameter] = classificationRegistry.GetClassificationType(ColoringConstants.BQLParameterFormat),
				[PXCodeType.BqlOperator]  = bqlClassificationType,
				[PXCodeType.BqlCommand]   = bqlClassificationType,

				[PXCodeType.BQLConstantPrefix] = classificationRegistry.GetClassificationType(ColoringConstants.BQLConstantPrefixFormat),
				[PXCodeType.BQLConstantEnding] = classificationRegistry.GetClassificationType(ColoringConstants.BQLConstantEndingFormat),

				[PXCodeType.PXGraph]  = classificationRegistry.GetClassificationType(ColoringConstants.PXGraphFormat),
				[PXCodeType.PXAction] = classificationRegistry.GetClassificationType(ColoringConstants.PXActionFormat),
			};

			return acumaticaCodeElementsClassificationTypes;
		}

		private static Dictionary<int, IClassificationType> GetClassificationTypesForAngleBraces(IClassificationTypeRegistryService classificationRegistry)
		{
			var braceTypeByLevel = new Dictionary<int, IClassificationType>(capacity: ColoringConstants.MaxBraceLevel)
			{
				[0] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_1_Format),
				[1] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_2_Format),
				[2] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_3_Format),

				[3] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_4_Format),
				[4] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_5_Format),
				[5] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_6_Format),

				[6] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_7_Format),
				[7] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_8_Format),
				[8] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_9_Format),

				[9]  = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_10_Format),
				[10] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_11_Format),
				[11] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_12_Format),

				[12] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_13_Format),
				[13] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_14_Format),
			};

			return braceTypeByLevel;
		}

		private static void IncreaseCommentFormatTypesPriority(IClassificationTypeRegistryService registry, IClassificationFormatMapService formatMapService,
															   IClassificationType highestPriorityType)
		{
			if (_isPriorityIncreased)
				return;

			bool lockTaken = false;
			Monitor.TryEnter(_syncRoot, ref lockTaken);

			if (!lockTaken)
				return;

			try
			{
				if (_isPriorityIncreased)
					return;

				if (formatMapService.GetClassificationFormatMap(category: TextCategory) is IClassificationFormatMap formatMap)
				{
					IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.ExcludedCode, highestPriorityType);
					IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.Comment, highestPriorityType);
					_isPriorityIncreased = true;
				}
			}
			finally
			{
				Monitor.Exit(_syncRoot);
			}
		}

		private static void IncreaseServiceFormatPriority(IClassificationFormatMap formatMap, IClassificationTypeRegistryService registry, string formatName,
														  IClassificationType highestPriorityType)
		{
			IClassificationType predefinedClassificationType = registry.GetClassificationType(formatName);
			IClassificationType artificialClassType = registry.CreateTransientClassificationType(predefinedClassificationType);
			TextFormattingRunProperties properties = formatMap.GetExplicitTextProperties(predefinedClassificationType);

			formatMap.AddExplicitTextProperties(artificialClassType, properties, highestPriorityType);
			formatMap.SwapPriorities(artificialClassType, predefinedClassificationType);
			formatMap.SwapPriorities(highestPriorityType, predefinedClassificationType);
		}
	}
}
