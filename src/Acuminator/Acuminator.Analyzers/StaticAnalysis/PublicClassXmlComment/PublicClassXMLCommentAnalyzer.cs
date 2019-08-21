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

            var isPublicClass = classDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword);
            if (!isPublicClass)
            {
                return;
            }

            var isAcumaticaClass = IsInAcumaticaRootNamespace(classDeclarationSyntax, syntaxContext.SemanticModel, syntaxContext.CancellationToken);
            if (!isAcumaticaClass)
            {
                return;
            }

            CheckXmlComment(syntaxContext, classDeclarationSyntax);
        }

        private bool IsInAcumaticaRootNamespace(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var classDeclarationSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken);
            if (classDeclarationSymbol == null)
            {
                return false;
            }

			var classRootNamespace = classDeclarationSymbol
				.GetContainingNamespaces()
				.Where(n => !string.IsNullOrEmpty(n.Name))
				.Last();
            if (!NamespaceNames.AcumaticaRootNamespace.Equals(classRootNamespace.Name, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
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
