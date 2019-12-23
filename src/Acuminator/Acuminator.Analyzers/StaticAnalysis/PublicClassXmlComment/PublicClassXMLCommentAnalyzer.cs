﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PublicClassXmlCommentAnalyzer : PXDiagnosticAnalyzer
	{
		internal enum FixOption
		{
			NoXmlComment,
			NoSummaryTag,
			EmptySummaryTag
		}

		public const string XmlCommentExcludeTag = "exclude";
		public static readonly string XmlCommentSummaryTag = SyntaxFactory.XmlSummaryElement().StartTag.Name.ToFullString();
		internal const string FixOptionKey = nameof(FixOption);

		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };

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

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			base.ShouldAnalyze(pxContext) && 
			pxContext.CodeAnalysisSettings.PX1007DocumentationDiagnosticEnabled;

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
			private readonly CodeAnalysisSettings _codeAnalysisSettings;
			private readonly Stack<bool> _isInsideDacContextStack = new Stack<bool>(2);

			private bool _skipDiagnosticReporting;

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
				INamedTypeSymbol typeSymbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, _syntaxContext.CancellationToken);

				try
				{
					_skipDiagnosticReporting = typeSymbol?.IsDacField(_pxContext) ?? false;

					if (!CheckXmlCommentAndTheNeedToGoToChildrenNodes(classDeclaration, classDeclaration.Modifiers, classDeclaration.Identifier))
						return;
				}
				finally
				{
					_skipDiagnosticReporting = false;
				}

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

				if (!modifiers.Any(SyntaxKind.PublicKeyword) || CheckIfMemberAttributesDisableDiagnostic(memberDeclaration))
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

			private bool CheckIfMemberAttributesDisableDiagnostic(MemberDeclarationSyntax member)
			{
				const string ObsoleteAttributeShortName = "Obsolete";
				const string PXHiddenAttributeShortName = "PXHidden";
				const string PXInternalUseOnlyAttributeShortName = "PXInternalUseOnly";
				const string AttributeSuffix = "Attribute";

				return member.GetAttributes()
							 .Select(attr => GetAttributeShortName(attr))
							 .Any(attrName => attrName == ObsoleteAttributeShortName ||
											  attrName == PXHiddenAttributeShortName ||
											  attrName == PXInternalUseOnlyAttributeShortName);

				//-------------------------Local Function-------------------------------------------
				string GetAttributeShortName(AttributeSyntax attribute)
				{
					string attributeShortName = attribute.Name is QualifiedNameSyntax qualifiedName
						? qualifiedName.Right.ToString()
						: attribute.Name.ToString();

					const int minLengthWithSuffix = 17;

					// perfomance optimization to avoid checking the suffix of attribute names 
					// which are definitely shorter than any of the attributes we search with "Attribute" suffix
					if (attributeShortName.Length >= minLengthWithSuffix && attributeShortName.EndsWith(AttributeSuffix))
					{
						const int suffixLength = 9;
						attributeShortName = attributeShortName.Substring(0, attributeShortName.Length - suffixLength);
					}

					return attributeShortName;
				}
			}

			private IEnumerable<DocumentationCommentTriviaSyntax> GetXmlComments(MemberDeclarationSyntax member) =>
				member.GetLeadingTrivia()
					  .Select(t => t.GetStructure())
					  .OfType<DocumentationCommentTriviaSyntax>();
			
			private XmlEmptyElementSyntax GetXmlExcludeTag(DocumentationCommentTriviaSyntax xmlComment) =>
				xmlComment
						  .ChildNodes()
						  .OfType<XmlEmptyElementSyntax>()
						  .FirstOrDefault(s => XmlCommentExcludeTag.Equals(s.Name?.ToString(), StringComparison.Ordinal));

			private XmlElementSyntax GetSummaryTag(DocumentationCommentTriviaSyntax xmlComment) =>
				xmlComment
						  .ChildNodes()
						  .OfType<XmlElementSyntax>()
						  .FirstOrDefault(n => XmlCommentSummaryTag.Equals(n.StartTag?.Name?.ToString(), StringComparison.Ordinal));

			private bool CommentContentIsNotEmpty(string content) =>
				!content.IsNullOrEmpty() && 
				 content.Any(char.IsLetterOrDigit);

			private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, MemberDeclarationSyntax memberDeclaration,
										  Location location, FixOption fixOption)
			{
				syntaxContext.CancellationToken.ThrowIfCancellationRequested();

				if (_skipDiagnosticReporting)
					return;

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
