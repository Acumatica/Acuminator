using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Analyzers.Settings.OutOfProcess;
using Acuminator.Analyzers.Utils;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Acuminator.Analyzers.Refactorings
{
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

		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			AcuminatorVsixPackageLoader.EnsurePackageLoaded();

			if (!_settingsProvidedExternally)
				CodeAnalysisSettings = AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisSettings(); //Initialize settings from global values after potential package load

			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
				return;

			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			if (semanticModel?.Compilation == null)
				return;

			var pxContext = new PXContext(semanticModel.Compilation, CodeAnalysisSettings);

			if (ShouldAnalyze(semanticModel, pxContext))
			{
				await ComputeRefactoringsAsync(context, semanticModel, pxContext).ConfigureAwait(false);
			}
		}

		protected virtual bool ShouldAnalyze(SemanticModel semanticModel, PXContext pxContext) => pxContext.IsPlatformReferenced &&
																								  pxContext.CodeAnalysisSettings.StaticAnalysisEnabled &&
																								 !pxContext.Compilation.IsUnitTestAssembly();

		protected abstract Task ComputeRefactoringsAsync(CodeRefactoringContext context, SemanticModel semanticModel, PXContext pxContext);
	}
}
