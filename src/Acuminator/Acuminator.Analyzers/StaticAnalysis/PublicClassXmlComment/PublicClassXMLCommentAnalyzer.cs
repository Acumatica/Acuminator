
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PublicClassXmlCommentAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			new[]
			{
				Descriptors.PX1007_PublicClassNoXmlComment,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription
			}
			.ToImmutableArray();

		public PublicClassXmlCommentAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) :
										base(codeAnalysisSettings)
		{
		}

		public PublicClassXmlCommentAnalyzer() : 
										this(null)
		{
		}

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			pxContext.CodeAnalysisSettings.PX1007DocumentationDiagnosticEnabled &&
			base.ShouldAnalyze(pxContext);
			
		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeCompilationUnit(context, pxContext),
															 SyntaxKind.CompilationUnit);
		}

		private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is CompilationUnitSyntax compilationUnitSyntax)
			{
				var commentsWalker = new XmlCommentsWalker(syntaxContext, pxContext, CodeAnalysisSettings!);
				compilationUnitSyntax.Accept(commentsWalker);
			}
		}
	}
}
