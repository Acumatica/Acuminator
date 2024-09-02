using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Acuminator.Analyzers.Settings.OutOfProcess;
using Acuminator.Analyzers.Utils;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		[MemberNotNullWhen(returnValue: true, nameof(CodeAnalysisSettings))]
		protected bool SettingsProvidedExternally { get; }

		protected CodeAnalysisSettings? CodeAnalysisSettings 
		{ 
			get;
			set;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="codeAnalysisSettings">(Optional) The code analysis settings for unit tests.</param>
		protected PXDiagnosticAnalyzer(CodeAnalysisSettings? codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings;
			SettingsProvidedExternally = codeAnalysisSettings != null;

			try
			{
				AcuminatorVsixPackageLoader.EnsurePackageLoaded();
			}
			catch (Exception e)
			{
			}
		}

		[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1025:Configure generated code analysis", 
						 Justification = $"Configured in the {nameof(ConfigureAnalysisContext)} method")]
		[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1026:Enable concurrent execution", 
						 Justification = $"Configured in the {nameof(ConfigureAnalysisContext)} method")]
		public override void Initialize(AnalysisContext context)
		{
			if (!SettingsProvidedExternally)
				CodeAnalysisSettings = AnalyzersOutOfProcessSettingsProvider.GetCodeAnalysisSettings(); //Initialize settings from global values from the shared memory

			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
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

		protected abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);
	}
}
