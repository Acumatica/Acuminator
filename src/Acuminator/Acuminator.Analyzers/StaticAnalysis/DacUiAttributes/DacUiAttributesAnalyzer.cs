using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacUiAttributes
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacUiAttributesAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1094_DacShouldHaveUiAttribute);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeDacUiAttributes(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeDacUiAttributes(SyntaxNodeAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();


		}
	}
}
