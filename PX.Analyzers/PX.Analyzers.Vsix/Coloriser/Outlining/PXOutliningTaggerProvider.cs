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
    //[ContentType("CSharp")]
    //[TagType(typeof(IOutliningRegionTag))]
    //[TextViewRole(PredefinedTextViewRoles.Document)]
    //[Export(typeof(IViewTaggerProvider))]
    //public class PXOutliningTaggerProvider : PXColorizerTaggerProvider
    //{                    
    //    protected override ITagger<T> CreateTaggerImpl<T>(ITextView textView, ITextBuffer textBuffer)
    //    {
    //        return new PXColorizerMainTagger(textBuffer, this, subscribeToSettingsChanges: true,
    //                                         useCacheChecking: true) as ITagger<T>;
    //    }       
    //}
}
