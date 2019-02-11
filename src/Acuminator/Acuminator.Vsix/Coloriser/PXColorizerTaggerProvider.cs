using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Roslyn;


namespace Acuminator.Vsix.Coloriser
{
    [ContentType(LegacyLanguageNames.CSharp)]
    [TagType(typeof(IClassificationTag))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Export(typeof(IViewTaggerProvider))]
    public class PXColorizerTaggerProvider : PXTaggerProviderBase, IViewTaggerProvider
    {
        [Import]
        internal IClassificationTypeRegistryService _classificationRegistry = null; // Set via MEF

        [Import]
        internal IClassificationFormatMapService _classificationFormatMapService = null;  //Set via MEF

        private const string TextCategory = "text";
        private static object _syncRoot = new object();
        private static bool _isPriorityIncreased;

        protected bool AreClassificationsInitialized
		{
			get;
			private set;
		}

        private Dictionary<PXCodeType, IClassificationType> _codeColoringClassificationTypes;

        public IClassificationType this[PXCodeType codeType]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _codeColoringClassificationTypes.TryGetValue(codeType, out IClassificationType type)
                 ? type
                 : null;
            }
        }

        private Dictionary<int, IClassificationType> _braceTypeByLevel;
    
        public IClassificationType this[int braceLevel]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
               return _braceTypeByLevel.TryGetValue(braceLevel, out IClassificationType type)
                ? type
                : null;
            }
        }

        public virtual ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer textBuffer)
        where T : ITag
        {
            Initialize(textBuffer);

            if (textView.TextBuffer != textBuffer || !IsInitialized || !HasReferenceToAcumaticaPlatform)
                return null;

            var tagger = textBuffer.Properties.GetOrCreateSingletonProperty(typeof(PXColorizerTaggerBase), () =>
            {
                return new PXColorizerMainTagger(textBuffer, this, subscribeToSettingsChanges: true, useCacheChecking: true);
            });

            return tagger as ITagger<T>;
        }
          
        
        protected override void Initialize(ITextBuffer textBuffer)
        {
            base.Initialize(textBuffer);

            if (AreClassificationsInitialized)
                return;

            AreClassificationsInitialized = true;
            InitializeClassificationTypes();          
            IncreaseCommentFormatTypesPrioirity(_classificationRegistry, _classificationFormatMapService, 
                                                _codeColoringClassificationTypes[PXCodeType.BqlParameter]);
        }

        protected void InitializeClassificationTypes()
        {
            IClassificationType bqlClassificationType = _classificationRegistry.GetClassificationType(ColoringConstants.BQLOperatorFormat);

            _codeColoringClassificationTypes = new Dictionary<PXCodeType, IClassificationType>
            {
                [PXCodeType.Dac]               = _classificationRegistry.GetClassificationType(ColoringConstants.DacFormat),
                [PXCodeType.DacExtension]      = _classificationRegistry.GetClassificationType(ColoringConstants.DacExtensionFormat),
                [PXCodeType.DacField]          = _classificationRegistry.GetClassificationType(ColoringConstants.DacFieldFormat),
                [PXCodeType.BqlParameter]      = _classificationRegistry.GetClassificationType(ColoringConstants.BQLParameterFormat),
                [PXCodeType.BqlOperator]       = bqlClassificationType,
                [PXCodeType.BqlCommand]        = bqlClassificationType,

                [PXCodeType.BQLConstantPrefix] = _classificationRegistry.GetClassificationType(ColoringConstants.BQLConstantPrefixFormat),
                [PXCodeType.BQLConstantEnding] = _classificationRegistry.GetClassificationType(ColoringConstants.BQLConstantEndingFormat),

                [PXCodeType.PXGraph]           = _classificationRegistry.GetClassificationType(ColoringConstants.PXGraphFormat),
                [PXCodeType.PXAction]          = _classificationRegistry.GetClassificationType(ColoringConstants.PXActionFormat),
            };

            _braceTypeByLevel = new Dictionary<int, IClassificationType>(capacity: ColoringConstants.MaxBraceLevel)
            {
                [0] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_1_Format),
                [1] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_2_Format),
                [2] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_3_Format),

                [3] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_4_Format),
                [4] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_5_Format),
                [5] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_6_Format),

                [6] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_7_Format),
                [7] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_8_Format),
                [8] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_9_Format),

                [9] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_10_Format),
                [10] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_11_Format),
                [11] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_12_Format),

                [12] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_13_Format),
                [13] = _classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_14_Format),
            }; 
        }

        private static void IncreaseCommentFormatTypesPrioirity(IClassificationTypeRegistryService registry, IClassificationFormatMapService formatMapService,
                                                               IClassificationType highestPriorityType)
        {
            bool lockTaken = false;
            Monitor.TryEnter(_syncRoot, ref lockTaken);

            if (lockTaken)
            {
                try
                {
                    if (!_isPriorityIncreased)
                    {
                        _isPriorityIncreased = true;
                        IClassificationFormatMap formatMap = formatMapService.GetClassificationFormatMap(category: TextCategory);
                        IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.ExcludedCode, highestPriorityType);
                        IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.Comment, highestPriorityType);
                    }
                }
                finally
                {
                    Monitor.Exit(_syncRoot);
                }
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
