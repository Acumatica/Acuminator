using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Analyzers.StaticAnalysis.CallsToInternalAPI
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class CallsToInternalAPIAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV);

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled && base.ShouldAnalyze(pxContext);

		public CallsToInternalAPIAnalyzer() : this(null)
		{ }

		public CallsToInternalAPIAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeCompilationUnit(context, pxContext),
															 SyntaxKind.CompilationUnit);
		}



		private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(syntaxContext.Node is CompilationUnitSyntax compilationUnitSyntax))
				return;

			var commentsWalker = new XmlCommentsWalker(syntaxContext, pxContext, CodeAnalysisSettings);
			compilationUnitSyntax.Accept(commentsWalker);
		}

	
	}
}
