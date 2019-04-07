using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using CommonServiceLocator;
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
		public PXDiagnosticAnalyzer(CodeAnalysisSettings codeAnalysisSettings = null)
		{
			CodeAnalysisSettings = codeAnalysisSettings ?? GlobalCodeAnalysisSettings.Instance;
		}

		public override void Initialize(AnalysisContext context)
		{
			if (!CodeAnalysisSettings.StaticAnalysisEnabled)
				return;

			//context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(compilationStartContext =>
			{
				var pxContext = new PXContext(compilationStartContext.Compilation);

				if (ShouldAnalyze(pxContext))
				{
					AnalyzeCompilation(compilationStartContext, pxContext);
				}
			});
		}

		protected virtual bool ShouldAnalyze(PXContext pxContext) =>  pxContext.IsPlatformReferenced;

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);	
	}
}
