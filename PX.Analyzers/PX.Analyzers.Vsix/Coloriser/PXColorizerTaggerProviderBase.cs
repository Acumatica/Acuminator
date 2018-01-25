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
    public abstract class PXColorizerTaggerProviderBase : IViewTaggerProvider, ITaggerProvider
    {    
        protected bool IsInitialized { get; set; }
        
        public AcuminatorVSPackage Package { get; protected set; }

		public virtual ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer textBuffer)
        where T : ITag
		{            
            Initialize();
            
            if (textView.TextBuffer != textBuffer)
                return null;

            Func<ITagger<T>> taggerFactory = () => CreateTaggerImpl<T>(textBuffer);
            return textBuffer.Properties.GetOrCreateSingletonProperty(taggerFactory);
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer textBuffer) where T : ITag
        {
            Initialize();

            Func<ITagger<T>> taggerFactory = () => CreateTaggerImpl<T>(textBuffer);
            return textBuffer.Properties.GetOrCreateSingletonProperty<ITagger<T>>(taggerFactory);
        }

        protected abstract ITagger<T> CreateTaggerImpl<T>(ITextBuffer textBuffer)
        where T : ITag;
        

        protected virtual void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            InitializePackage();
        }

        protected virtual void InitializePackage()
        {
            IVsShell shellService = ServiceProvider.GlobalProvider.GetService(typeof(IVsShell)) as IVsShell;

            if (shellService == null)
            {
                IsInitialized = false;
                return;
            }
            
            Guid acuminatorGUID = Guid.Parse(AcuminatorVSPackage.PackageGuidString);
            int returnCode = shellService.IsPackageLoaded(ref acuminatorGUID, out IVsPackage package);

            if (returnCode != Microsoft.VisualStudio.VSConstants.S_OK)
            {
                shellService.LoadPackage(ref acuminatorGUID, out package);
            }
            
            Package = package as AcuminatorVSPackage;

            if (Package == null)
                throw new Exception("Acuminator package loaded incorrectly");
        }
    }
}
