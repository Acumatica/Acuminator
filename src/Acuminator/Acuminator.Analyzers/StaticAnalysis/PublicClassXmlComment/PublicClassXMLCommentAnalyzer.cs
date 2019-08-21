using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using static Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.PublicClassXmlCommentFix;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PublicClassXmlCommentAnalyzer : PXDiagnosticAnalyzer
	{
		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };

		public static readonly string XmlCommentSummaryTag = SyntaxFactory.XmlSummaryElement().StartTag.Name.ToFullString();

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1007_PublicClassXmlComment);

		public PublicClassXmlCommentAnalyzer(CodeAnalysisSettings codeAnalysisSettings)
			: base(codeAnalysisSettings)
		{
		}

		public PublicClassXmlCommentAnalyzer()
			: this(null)
		{
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext syntaxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(syntaxContext.Node is ClassDeclarationSyntax classDeclarationSyntax))
			{
				return;
			}

			var classDeclarationSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax, syntaxContext.CancellationToken);
			if (classDeclarationSymbol == null)
			{
				return;
			}

			var isPublicClass = classDeclarationSymbol
				.GetContainingTypesAndThis()
				.All(t => t.DeclaredAccessibility == Accessibility.Public);
			if (!isPublicClass)
			{
				return;
			}

			var isAcumaticaClass = classDeclarationSymbol.IsInAcumaticaRootNamespace();
			if (!isAcumaticaClass)
			{
				return;
			}

			CheckXmlComment(syntaxContext, classDeclarationSyntax);
		}

		private void CheckXmlComment(SyntaxNodeAnalysisContext syntaxContext, ClassDeclarationSyntax classDeclarationSyntax)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!classDeclarationSyntax.HasStructuredTrivia)
			{
				ReportDiagnostic(syntaxContext, classDeclarationSyntax, FixOption.NoXmlComment);
				return;
			}

			var xmlComment = classDeclarationSyntax
				.GetLeadingTrivia()
				.Select(t => t.GetStructure())
				.OfType<DocumentationCommentTriviaSyntax>()
				.FirstOrDefault();
			if (xmlComment == null)
			{
				ReportDiagnostic(syntaxContext, classDeclarationSyntax, FixOption.NoXmlComment);
				return;
			}

			var excludeTag = xmlComment
				.ChildNodes()
				.OfType<XmlEmptyElementSyntax>()
				.Where(s => XmlCommentExcludeTag.Equals(s?.Name?.ToString(), StringComparison.Ordinal))
				.FirstOrDefault();
			if (excludeTag != null)
			{
				return;
			}

			var summaryTag = xmlComment
				.ChildNodes()
				.OfType<XmlElementSyntax>()
				.Where(n => XmlCommentSummaryTag.Equals(n?.StartTag?.Name?.ToString(), StringComparison.Ordinal))
				.FirstOrDefault();
			if (summaryTag == null)
			{
				ReportDiagnostic(syntaxContext, classDeclarationSyntax, FixOption.NoSummaryTag);
				return;
			}

			var summaryContent = summaryTag.Content;
			if (summaryContent.Count == 0)
			{
				ReportDiagnostic(syntaxContext, classDeclarationSyntax, FixOption.EmptySummaryTag);
				return;
			}

			foreach (var contentNode in summaryContent)
			{
				var contentString = contentNode.ToFullString();

				if (string.IsNullOrWhiteSpace(contentString))
				{
					continue;
				}

				var contentHasText = contentString.Split(_xmlCommentSummarySeparators, StringSplitOptions.RemoveEmptyEntries)
					.Any(s => !string.IsNullOrWhiteSpace(s));
				if (contentHasText)
				{
					return;
				}
			}

			ReportDiagnostic(syntaxContext, classDeclarationSyntax, FixOption.EmptySummaryTag);
		}

		private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, ClassDeclarationSyntax classDeclarationSyntax, FixOption fixOption)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var properties = ImmutableDictionary<string, string>.Empty
				.Add(FixOptionKey, fixOption.ToString());
			var noXmlCommentDiagnostic = Diagnostic.Create(
				Descriptors.PX1007_PublicClassXmlComment,
				classDeclarationSyntax.Identifier.GetLocation(),
				properties);

			syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, CodeAnalysisSettings);
		}
	}
}
