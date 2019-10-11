using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		protected CodeAnalysisSettings CodeAnalysisSettings { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="codeAnalysisSettings">(Optional) The code analysis settings for unit tests.</param>
		protected PXDiagnosticAnalyzer(CodeAnalysisSettings codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings ?? GlobalCodeAnalysisSettings.Instance;
		}
		
		public override void Initialize(AnalysisContext context)
		{
			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
				return;

			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

			//context.EnableConcurrentExecution();
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
																	  pxContext.CodeAnalysisSettings.StaticAnalysisEnabled;

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);	
	}
}
