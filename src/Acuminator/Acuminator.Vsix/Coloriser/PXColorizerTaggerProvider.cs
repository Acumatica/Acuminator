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
using Acuminator.Vsix;



namespace Acuminator.Vsix.Coloriser
{
    [ContentType("CSharp")]
    [TagType(typeof(IClassificationTag))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Export(typeof(IViewTaggerProvider))]
    public class PXColorizerTaggerProvider : PXTaggerProviderBase, IViewTaggerProvider
    {
        [Import]
        internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        [Import]
        internal IClassificationFormatMapService classificationFormatMapService = null;  //Set via MEF

        private const string textCategory = "text";
        private static object syncRoot = new object();
        private static bool isPriorityIncreased;

        protected bool AreClassificationsInitialized { get; private set; }

        private Dictionary<ColoredCodeType, IClassificationType> codeColoringClassificationTypes;

        public IClassificationType this[ColoredCodeType codeType]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return codeColoringClassificationTypes.TryGetValue(codeType, out IClassificationType type)
                 ? type
                 : null;
            }
        }

        private Dictionary<int, IClassificationType> braceTypeByLevel;
    
        public IClassificationType this[int braceLevel]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
               return braceTypeByLevel.TryGetValue(braceLevel, out IClassificationType type)
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
            IncreaseCommentFormatTypesPrioirity(classificationRegistry, classificationFormatMapService, 
                                                codeColoringClassificationTypes[ColoredCodeType.BqlParameter]);
        }

        protected void InitializeClassificationTypes()
        {
            IClassificationType bqlClassificationType = classificationRegistry.GetClassificationType(ColoringConstants.BQLOperatorFormat);

            codeColoringClassificationTypes = new Dictionary<ColoredCodeType, IClassificationType>
            {
                [ColoredCodeType.Dac]               = classificationRegistry.GetClassificationType(ColoringConstants.DacFormat),
                [ColoredCodeType.DacExtension]      = classificationRegistry.GetClassificationType(ColoringConstants.DacExtensionFormat),
                [ColoredCodeType.DacField]          = classificationRegistry.GetClassificationType(ColoringConstants.DacFieldFormat),
                [ColoredCodeType.BqlParameter]      = classificationRegistry.GetClassificationType(ColoringConstants.BQLParameterFormat),
                [ColoredCodeType.BqlOperator]       = bqlClassificationType,
                [ColoredCodeType.BqlCommand]        = bqlClassificationType,

                [ColoredCodeType.BQLConstantPrefix] = classificationRegistry.GetClassificationType(ColoringConstants.BQLConstantPrefixFormat),
                [ColoredCodeType.BQLConstantEnding] = classificationRegistry.GetClassificationType(ColoringConstants.BQLConstantEndingFormat),

                [ColoredCodeType.PXGraph]           = classificationRegistry.GetClassificationType(ColoringConstants.PXGraphFormat),
                [ColoredCodeType.PXAction]          = classificationRegistry.GetClassificationType(ColoringConstants.PXActionFormat),
            };

            braceTypeByLevel = new Dictionary<int, IClassificationType>(capacity: ColoringConstants.MaxBraceLevel)
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

                [9] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_10_Format),
                [10] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_11_Format),
                [11] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_12_Format),

                [12] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_13_Format),
                [13] = classificationRegistry.GetClassificationType(ColoringConstants.BraceLevel_14_Format),
            }; 
        }

        private static void IncreaseCommentFormatTypesPrioirity(IClassificationTypeRegistryService registry, IClassificationFormatMapService formatMapService,
                                                               IClassificationType highestPriorityType)
        {
            bool lockTaken = false;
            Monitor.TryEnter(syncRoot, ref lockTaken);

            if (lockTaken)
            {
                try
                {
                    if (!isPriorityIncreased)
                    {
                        isPriorityIncreased = true;
                        IClassificationFormatMap formatMap = formatMapService.GetClassificationFormatMap(category: textCategory);
                        IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.ExcludedCode, highestPriorityType);
                        IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.Comment, highestPriorityType);
                    }
                }
                finally
                {
                    Monitor.Exit(syncRoot);
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
