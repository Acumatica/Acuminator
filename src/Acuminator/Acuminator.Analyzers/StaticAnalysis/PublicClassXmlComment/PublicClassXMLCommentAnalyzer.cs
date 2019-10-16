using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

		public PublicClassXmlCommentAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{
		}

		public PublicClassXmlCommentAnalyzer() : this(null)
		{
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
		}

		private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext syntaxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(syntaxContext.Node is CompilationUnitSyntax compilationUnitSyntax))
				return;

			var commentsWalker = new XmlCommentsWalker(syntaxContext, CodeAnalysisSettings);
			compilationUnitSyntax.Accept(commentsWalker);
		}

		private class XmlCommentsWalker : CSharpSyntaxWalker
		{
			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private CodeAnalysisSettings _codeAnalysisSettings;
			private bool _isInsideAcumaticaNamespace;

			public XmlCommentsWalker(SyntaxNodeAnalysisContext syntaxContext, CodeAnalysisSettings codeAnalysisSettings)
			{
				_syntaxContext = syntaxContext;
				_codeAnalysisSettings = codeAnalysisSettings.CheckIfNull(nameof(codeAnalysisSettings));
			}

			public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax namespaceDeclaration)
			{
				if (_isInsideAcumaticaNamespace)    //for nested namespace declarations
				{
					base.VisitNamespaceDeclaration(namespaceDeclaration);
					return;
				}

				try
				{
					string namespaceName = namespaceDeclaration.Name?.ToString();
					bool startWith = namespaceName?.StartsWith(NamespaceNames.AcumaticaRootNamespaceWithDot, StringComparison.Ordinal) ?? false;
					_isInsideAcumaticaNamespace = startWith || NamespaceNames.AcumaticaRootNamespace == namespaceName;

					if (_isInsideAcumaticaNamespace)
					{
						base.VisitNamespaceDeclaration(namespaceDeclaration);
					}
				}
				finally
				{

					_isInsideAcumaticaNamespace = false;
				}
			}

			public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
			{
				if (CheckXmlCommentAndTheNeedToGoToChildrenNodes(classDeclaration, classDeclaration.Modifiers, classDeclaration.Identifier))
				{
					base.VisitClassDeclaration(classDeclaration);
				}
			}

			public override void VisitStructDeclaration(StructDeclarationSyntax structDeclaration)
			{
				if (CheckXmlCommentAndTheNeedToGoToChildrenNodes(structDeclaration, structDeclaration.Modifiers, structDeclaration.Identifier))
				{
					base.VisitStructDeclaration(structDeclaration);
				}
			}

			public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDeclaration)
			{
				if (CheckXmlCommentAndTheNeedToGoToChildrenNodes(interfaceDeclaration, interfaceDeclaration.Modifiers,
					interfaceDeclaration.Identifier))
				{
					base.VisitInterfaceDeclaration(interfaceDeclaration);
				}
			}

			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax delegateDeclaration)
			{
				if (CheckXmlCommentAndTheNeedToGoToChildrenNodes(delegateDeclaration, delegateDeclaration.Modifiers,
																 delegateDeclaration.Identifier))
				{
					base.VisitDelegateDeclaration(delegateDeclaration);
				}
			}

			public override void VisitEnumDeclaration(EnumDeclarationSyntax enumDeclaration)
			{
				if (CheckXmlCommentAndTheNeedToGoToChildrenNodes(enumDeclaration, enumDeclaration.Modifiers, enumDeclaration.Identifier))
				{
					base.VisitEnumDeclaration(enumDeclaration);
				}
			}

			private bool CheckXmlCommentAndTheNeedToGoToChildrenNodes(MemberDeclarationSyntax memberDeclaration, SyntaxTokenList modifiers,
																	  SyntaxToken identifier)
			{
				_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

				//extra check for _isInsideAcumaticaNamespace for classes declared in a global namespace
				if (!_isInsideAcumaticaNamespace || !modifiers.Any(SyntaxKind.PublicKeyword))
					return false;

				if (!memberDeclaration.HasStructuredTrivia)
				{
					ReportDiagnostic(_syntaxContext, identifier.GetLocation(), FixOption.NoXmlComment);
					return true;
				}

				IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(memberDeclaration);
				bool hasXmlComment = false, hasSummaryTag = false, nonEmptySummaryTag = false;

				foreach (DocumentationCommentTriviaSyntax xmlComment in xmlComments)
				{
					hasXmlComment = true;
					var excludeTag = GetXmlExcludeTag(xmlComment);

					if (excludeTag != null)
						return false;
					else if (hasSummaryTag)
						continue;

					var summaryTag = GetSummaryTag(xmlComment);
					hasSummaryTag = summaryTag != null;

					if (!hasSummaryTag)
						continue;

					var summaryContent = summaryTag.Content;

					if (summaryContent.Count == 0)
						continue;

					foreach (XmlNodeSyntax contentNode in summaryContent)
					{
						var contentString = contentNode.ToFullString();
						if (contentString.IsNullOrWhiteSpace())
							continue;

						var contentHasText = contentString.Split(_xmlCommentSummarySeparators, StringSplitOptions.RemoveEmptyEntries)
														  .Any(CommentContentIsNotEmpty);
						if (contentHasText)
						{
							nonEmptySummaryTag = true;
							break;
						}
					}
				}

				if (!hasXmlComment)
				{
					ReportDiagnostic(_syntaxContext, identifier.GetLocation(), FixOption.NoXmlComment);		
				}
				else if (!hasSummaryTag)
				{
					ReportDiagnostic(_syntaxContext, identifier.GetLocation(), FixOption.NoSummaryTag);
				}
				else if (!nonEmptySummaryTag)
				{
					ReportDiagnostic(_syntaxContext, identifier.GetLocation(), FixOption.EmptySummaryTag);
				}

				return true;
			}

			private IEnumerable<DocumentationCommentTriviaSyntax> GetXmlComments(MemberDeclarationSyntax member) =>
				member.GetLeadingTrivia()
					  .Select(t => t.GetStructure())
					  .OfType<DocumentationCommentTriviaSyntax>();
			
			private XmlEmptyElementSyntax GetXmlExcludeTag(DocumentationCommentTriviaSyntax xmlComment) =>
				xmlComment.ChildNodes()
						  .OfType<XmlEmptyElementSyntax>()
						  .Where(s => XmlCommentExcludeTag.Equals(s?.Name?.ToString(), StringComparison.Ordinal))
						  .FirstOrDefault();

			private XmlElementSyntax GetSummaryTag(DocumentationCommentTriviaSyntax xmlComment) =>
				xmlComment.ChildNodes()
						  .OfType<XmlElementSyntax>()
						  .Where(n => XmlCommentSummaryTag.Equals(n?.StartTag?.Name?.ToString(), StringComparison.Ordinal))
						  .FirstOrDefault();

			private bool CommentContentIsNotEmpty(string content) =>
				!content.IsNullOrEmpty() && 
				 content.Any(c => char.IsLetterOrDigit(c));

			private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, Location location, FixOption fixOption)
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();
				var properties = ImmutableDictionary<string, string>.Empty
																	.Add(FixOptionKey, fixOption.ToString());
				var noXmlCommentDiagnostic = Diagnostic.Create(Descriptors.PX1007_PublicClassXmlComment, location, properties);

				syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, _codeAnalysisSettings);
			}
		}
	}
}
