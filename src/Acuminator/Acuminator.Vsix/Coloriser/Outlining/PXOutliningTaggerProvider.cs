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
using Acuminator.Vsix;



namespace Acuminator.Vsix.Coloriser
{
    [ContentType("CSharp")]
    [TagType(typeof(IOutliningRegionTag))]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Export(typeof(ITaggerProvider))]
    public class PXOutliningTaggerProvider : PXTaggerProviderBase, ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null || !ThreadHelper.CheckAccess())
                return null;

            Initialize(buffer);

            if (!HasReferenceToAcumaticaPlatform)
                return null;

            PXOutliningTagger outliningTagger = buffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                return new PXOutliningTagger(buffer, this, subscribeToSettingsChanges: true, useCacheChecking: true);
            });

            return outliningTagger as ITagger<T>;
        }       
    }
}
