#nullable enable

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
		private readonly bool _settingsProvidedExternally;

		protected CodeAnalysisSettings? CodeAnalysisSettings 
		{ 
			get;
			private set;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="codeAnalysisSettings">(Optional) The code analysis settings for unit tests.</param>
		protected PXDiagnosticAnalyzer(CodeAnalysisSettings? codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings;
			_settingsProvidedExternally = codeAnalysisSettings != null;
		}
		
		public override void Initialize(AnalysisContext context)
		{
			AcuminatorVsixPackageLoader.EnsurePackageLoaded();

			if (!_settingsProvidedExternally)
				CodeAnalysisSettings = AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisSettings(); //Initialize settings from global values after potential package load

			if (!CodeAnalysisSettings!.StaticAnalysisEnabled)
				return;

			ConfigureAnalysisContext(context);

			context.RegisterCompilationStartAction(compilationStartContext =>
			{
				var pxContext = new PXContext(compilationStartContext.Compilation, CodeAnalysisSettings);

				if (ShouldAnalyze(pxContext))
				{
					AnalyzeCompilation(compilationStartContext, pxContext);
				}
			});
		}

		protected virtual void ConfigureAnalysisContext(AnalysisContext analysisContext)
		{
			analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

			if (!System.Diagnostics.Debugger.IsAttached)    // Disable concurrent execution during debug
			{
				analysisContext.EnableConcurrentExecution();
			}
		}

		protected virtual bool ShouldAnalyze(PXContext pxContext) =>  pxContext.IsPlatformReferenced && 
																	  pxContext.CodeAnalysisSettings.StaticAnalysisEnabled &&
																	  !pxContext.Compilation.IsUnitTestAssembly();

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);
	}
}
