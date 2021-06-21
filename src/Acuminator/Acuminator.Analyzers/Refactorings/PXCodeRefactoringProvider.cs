using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acuminator.Analyzers.Settings.OutOfProcess;
using Acuminator.Analyzers.Utils;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Acuminator.Analyzers.Refactorings.ChangeEventHandlerSignatureToGeneric
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp), Shared]
	public abstract class PXCodeRefactoringProvider : CodeRefactoringProvider
	{
		private readonly bool _settingsProvidedExternally;

		protected CodeAnalysisSettings CodeAnalysisSettings
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="codeAnalysisSettings">(Optional) The code analysis settings for unit tests.</param>
		protected PXCodeRefactoringProvider(CodeAnalysisSettings codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings;
			_settingsProvidedExternally = codeAnalysisSettings != null;
		}

		public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			AcuminatorVsixPackageLoader.EnsurePackageLoaded();

			if (!_settingsProvidedExternally)
				CodeAnalysisSettings = AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisSettings(); //Initialize settings from global values after potential package load

			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
				return;

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings);

			if (ShouldAnalyze(pxContext))
			{
				Analyze(context, pxContext);
			}
		}

		protected virtual bool ShouldAnalyze(PXContext pxContext) => pxContext.IsPlatformReferenced &&
																	 pxContext.CodeAnalysisSettings.StaticAnalysisEnabled &&
																	 !pxContext.Compilation.IsUnitTestAssembly();

		protected abstract void Analyze(CodeRefactoringContext context, PXContext pxContext);
	}
}
