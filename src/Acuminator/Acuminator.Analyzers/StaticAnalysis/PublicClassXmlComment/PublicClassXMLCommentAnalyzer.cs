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
using Acuminator.Utilities.Roslyn.Semantic.Dac;

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

		public PublicClassXmlCommentAnalyzer(CodeAnalysisSettings codeAnalysisSettings) :
										base(codeAnalysisSettings)
		{
		}

		public PublicClassXmlCommentAnalyzer() : 
										this(null)
		{
		}

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

		private class XmlCommentsWalker : CSharpSyntaxWalker
		{
			private readonly PXContext _pxContext;
			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private CodeAnalysisSettings _codeAnalysisSettings;
			private Stack<bool> _isInsideDacContextStack = new Stack<bool>(2);

			public XmlCommentsWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, 
									 CodeAnalysisSettings codeAnalysisSettings)
			{
				_syntaxContext = syntaxContext;
				_pxContext = pxContext;
				_codeAnalysisSettings = codeAnalysisSettings;
			}

			public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
			{
				// stop visitor for going into methods to improve performance
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
			{
				// stop visitor for going into methods to improve performance
			}

			public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
			{
				if (!CheckXmlCommentAndTheNeedToGoToChildrenNodes(classDeclaration, classDeclaration.Modifiers, classDeclaration.Identifier))
					return;

				INamedTypeSymbol typeSymbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, _syntaxContext.CancellationToken);

				try
				{
					bool isInsideDacOrDacExt = typeSymbol?.IsDacOrExtension(_pxContext) ?? false;
					_isInsideDacContextStack.Push(isInsideDacOrDacExt);
					base.VisitClassDeclaration(classDeclaration);
				}
				finally
				{
					_isInsideDacContextStack.Pop();
				}				
			}

			public override void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
			{
				bool isInsideDacOrDacExt = _isInsideDacContextStack.Count > 0
					? _isInsideDacContextStack.Peek()
					: false;

				if (!isInsideDacOrDacExt)
					return;

				CheckXmlCommentAndTheNeedToGoToChildrenNodes(propertyDeclaration, propertyDeclaration.Modifiers, propertyDeclaration.Identifier);
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
				if (!modifiers.Any(SyntaxKind.PublicKeyword))
					return false;

				if (!memberDeclaration.HasStructuredTrivia)
				{
					ReportDiagnostic(_syntaxContext, memberDeclaration, identifier.GetLocation(), FixOption.NoXmlComment);
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
					ReportDiagnostic(_syntaxContext, memberDeclaration, identifier.GetLocation(), FixOption.NoXmlComment);		
				}
				else if (!hasSummaryTag)
				{
					ReportDiagnostic(_syntaxContext, memberDeclaration, identifier.GetLocation(), FixOption.NoSummaryTag);
				}
				else if (!nonEmptySummaryTag)
				{
					ReportDiagnostic(_syntaxContext, memberDeclaration, identifier.GetLocation(), FixOption.EmptySummaryTag);
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

			private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, MemberDeclarationSyntax memberDeclaration,
										  Location location, FixOption fixOption)
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();
				var memberCategory = GetMemberCategory(memberDeclaration);
				var properties = ImmutableDictionary<string, string>.Empty
																	.Add(FixOptionKey, fixOption.ToString());
				var noXmlCommentDiagnostic = Diagnostic.Create(Descriptors.PX1007_PublicClassXmlComment, location, properties, memberCategory);

				syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, _codeAnalysisSettings);
			}

			private LocalizableString GetMemberCategory(MemberDeclarationSyntax memberDeclaration)
			{		
				switch (memberDeclaration)
				{
					case ClassDeclarationSyntax _:
						return nameof(Resources.PX1007Class).GetLocalized();
					case PropertyDeclarationSyntax _:
						return nameof(Resources.PX1007DacProperty).GetLocalized();
					case StructDeclarationSyntax _:
						return nameof(Resources.PX1007Struct).GetLocalized();
					case InterfaceDeclarationSyntax _:
						return nameof(Resources.PX1007Interface).GetLocalized();
					case EnumDeclarationSyntax _:
						return nameof(Resources.PX1007Enum).GetLocalized();
					case DelegateDeclarationSyntax _:
						return nameof(Resources.PX1007Delegate).GetLocalized();
					default:
						return nameof(Resources.PX1007DefaultEntity).GetLocalized();
				}
			}
		}
	}
}
