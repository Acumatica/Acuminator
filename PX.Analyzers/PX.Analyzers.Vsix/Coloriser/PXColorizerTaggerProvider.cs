using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
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
using PX.Analyzers.Vsix;



namespace PX.Analyzers.Coloriser
{
    [ContentType("CSharp")]
    [TagType(typeof(IClassificationTag))]
    [TagType(typeof(IOutliningRegionTag))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Export(typeof(IViewTaggerProvider))]
    [Export(typeof(ITaggerProvider))]
    public class PXColorizerTaggerProvider : PXColorizerTaggerProviderBase
    {    
        [Import]
        internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        [Import]
        internal IClassificationFormatMapService classificationFormatMapService = null;  //Set via MEF

        private const string textCategory = "text";
        private static object syncRoot = new object();
        private static bool isPriorityIncreased;

        public IClassificationType DacType { get; protected set; }

        public IClassificationType DacExtensionType { get; protected set; }

        public IClassificationType FieldType { get; protected set; }

        public IClassificationType BqlParameterType { get; protected set; }

        public IClassificationType BqlOperatorType { get; protected set; }

		public IClassificationType BqlConstantPrefixType { get; protected set; }

		public IClassificationType BqlConstantEndingType { get; protected set; }

        public Dictionary<int, IClassificationType> BraceTypeByLevel { get; protected set; }
       
        protected override ITagger<T> CreateTaggerImpl<T>(ITextBuffer textBuffer)
        {
            return new PXColorizerMainTagger(textBuffer, this, subscribeToSettingsChanges: true,
                                             useCacheChecking: true) as ITagger<T>;
        }

        protected override void Initialize()
        {
            if (IsInitialized)
                return;

            base.Initialize();
            InitializeClassificationTypes();          
            IncreaseCommentFormatTypesPrioirity(classificationRegistry, classificationFormatMapService, BqlParameterType);
        }

        protected void InitializeClassificationTypes()
        {
            DacType = classificationRegistry.GetClassificationType(Constants.DacFormat);
            DacExtensionType = classificationRegistry.GetClassificationType(Constants.DacExtensionFormat);
            FieldType = classificationRegistry.GetClassificationType(Constants.DacFieldFormat);
            BqlParameterType = classificationRegistry.GetClassificationType(Constants.BQLParameterFormat);
            BqlOperatorType = classificationRegistry.GetClassificationType(Constants.BQLOperatorFormat);
			BqlConstantPrefixType = classificationRegistry.GetClassificationType(Constants.BQLConstantPrefixFormat);
			BqlConstantEndingType = classificationRegistry.GetClassificationType(Constants.BQLConstantEndingFormat);

            BraceTypeByLevel = new Dictionary<int, IClassificationType>(capacity: Constants.MaxBraceLevel)
            {
                [1] = classificationRegistry.GetClassificationType(Constants.BraceLevel_1_Format),
                [2] = classificationRegistry.GetClassificationType(Constants.BraceLevel_2_Format),
                [3] = classificationRegistry.GetClassificationType(Constants.BraceLevel_3_Format),

                [4] = classificationRegistry.GetClassificationType(Constants.BraceLevel_4_Format),
                [5] = classificationRegistry.GetClassificationType(Constants.BraceLevel_5_Format),
                [6] = classificationRegistry.GetClassificationType(Constants.BraceLevel_6_Format),

                [7] = classificationRegistry.GetClassificationType(Constants.BraceLevel_7_Format),
                [8] = classificationRegistry.GetClassificationType(Constants.BraceLevel_8_Format),
                [9] = classificationRegistry.GetClassificationType(Constants.BraceLevel_9_Format),

                [10] = classificationRegistry.GetClassificationType(Constants.BraceLevel_10_Format),
                [11] = classificationRegistry.GetClassificationType(Constants.BraceLevel_11_Format),
                [12] = classificationRegistry.GetClassificationType(Constants.BraceLevel_12_Format),

                [13] = classificationRegistry.GetClassificationType(Constants.BraceLevel_13_Format),
                [14] = classificationRegistry.GetClassificationType(Constants.BraceLevel_14_Format),
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
