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
	[Export(typeof(ITaggerProvider))]
	[ContentType("CSharp")]
	[TagType(typeof(IClassificationTag))]
	internal class PXColorizerTaggerProvider : ITaggerProvider
	{
        private const string textCategory = "text";

        public IClassificationType DacType { get; private set; }

        public IClassificationType FieldType { get; private set; }

        public IClassificationType BqlParameterType { get; private set; }

        public IClassificationType BqlOperatorType { get; private set; }

        private bool isInitialized;
        private static object syncRoot = new object();
        private static bool isPriorityIncreased;

        [Import]
		internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        [Import]
        internal IClassificationFormatMapService classificationFormatMapService = null;  //Set via MEF

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer)
		where T : ITag
		{
            if (!isInitialized)
            {
                isInitialized = true;

                DacType = classificationRegistry.GetClassificationType(Constants.DacFormat);
                FieldType = classificationRegistry.GetClassificationType(Constants.DacFieldFormat);
                BqlParameterType = classificationRegistry.GetClassificationType(Constants.BQLParameterFormat);
                BqlOperatorType = classificationRegistry.GetClassificationType(Constants.BQLOperatorFormat);
            }

            IncreaseCommentFormatTypesPrioirity(classificationRegistry, classificationFormatMapService, BqlParameterType);
            return (ITagger<T>)new PXColorizerTagger(buffer, this);
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
