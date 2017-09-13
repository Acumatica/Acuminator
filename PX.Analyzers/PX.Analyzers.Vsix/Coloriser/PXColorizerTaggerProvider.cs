using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
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

		[Import]
		internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        [Import]
        internal IClassificationFormatMapService classificationFormatMapServise = null;  //Set via MEF

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer)
		where T : ITag
		{
            IClassificationFormatMap formatMap = classificationFormatMapServise.GetClassificationFormatMap(category: textCategory);
            return (ITagger<T>)new PXColorizerTagger(buffer, classificationRegistry, formatMap);
		}
	}
}
