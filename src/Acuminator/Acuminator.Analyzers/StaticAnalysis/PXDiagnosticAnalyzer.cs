using System;
using System.Linq;
using System.Collections.Generic;

using Acuminator.Analyzers.Settings.OutOfProcess;
using Acuminator.Analyzers.Utils;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis.Diagnostics;


namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		private bool _settingsProvidedExternally;

		protected CodeAnalysisSettings CodeAnalysisSettings 
		{ 
			get;
			private set;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="codeAnalysisSettings">(Optional) The code analysis settings for unit tests.</param>
		protected PXDiagnosticAnalyzer(CodeAnalysisSettings codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings;
			_settingsProvidedExternally = codeAnalysisSettings != null;
		}
		
		public override void Initialize(AnalysisContext context)
		{
			AcuminatorVsixPackageLoader.EnsurePackageLoaded();

			if (!_settingsProvidedExternally)
				CodeAnalysisSettings = AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisSettings(); //Initialize settings form global values after potential package load

			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
				return;

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(compilationStartContext =>
			{
				var pxContext = new PXContext(compilationStartContext.Compilation, CodeAnalysisSettings);

				if (ShouldAnalyze(pxContext))
				{
					AnalyzeCompilation(compilationStartContext, pxContext);
				}
			});
		}

		protected virtual bool ShouldAnalyze(PXContext pxContext) =>  pxContext.IsPlatformReferenced && 
																	  pxContext.CodeAnalysisSettings.StaticAnalysisEnabled &&
																	  !pxContext.Compilation.IsUnitTestAssembly();

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);
	}
}
