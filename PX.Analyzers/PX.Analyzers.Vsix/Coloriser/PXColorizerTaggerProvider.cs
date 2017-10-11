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



namespace PX.Analyzers.Coloriser
{
    [ContentType("CSharp")]
    [TagType(typeof(IClassificationTag))]
    [Export(typeof(ITaggerProvider))]
    internal class PXColorizerTaggerProvider : ITaggerProvider
    {
        private static readonly bool useRegexColoring = false;

        [Import]
        internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        [Import]
        internal IClassificationFormatMapService classificationFormatMapService = null;  //Set via MEF

        private const string textCategory = "text";
        private static object syncRoot = new object();
        private static bool isPriorityIncreased;
        private bool isInitialized;

        public IClassificationType DacType { get; protected set; }

        public IClassificationType FieldType { get; protected set; }

        public IClassificationType BqlParameterType { get; protected set; }

        public IClassificationType BqlOperatorType { get; protected set; }    
      
		public ITagger<T> CreateTagger<T>(ITextBuffer buffer)
        where T : ITag
		{
            InitializeClassificationTypes();

            if (useRegexColoring)
            {
                IncreaseCommentFormatTypesPrioirity(classificationRegistry, classificationFormatMapService, BqlParameterType);
                return (ITagger<T>)new PXRegexColorizerTagger(buffer, this);
            }
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

        protected void InitializeClassificationTypes()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            DacType = classificationRegistry.GetClassificationType(Constants.DacFormat);
            FieldType = classificationRegistry.GetClassificationType(Constants.DacFieldFormat);
            BqlParameterType = classificationRegistry.GetClassificationType(Constants.BQLParameterFormat);
            BqlOperatorType = classificationRegistry.GetClassificationType(Constants.BQLOperatorFormat);
        }
    }
}
