using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal class XmlCommentsWalker : CSharpSyntaxWalker
	{
		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };

		private readonly PXContext _pxContext;
		private readonly SyntaxNodeAnalysisContext _syntaxContext;
		private readonly CodeAnalysisSettings _codeAnalysisSettings;
		private readonly Stack<bool> _isInsideDacContextStack = new Stack<bool>(2);

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

		public override void VisitStructDeclaration(StructDeclarationSyntax structDeclaration)
		{
			// stop visitor for going into structs to improve performance
		}

		public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDeclaration)
		{
			// stop visitor for going into interfaces to improve performance
		}

		public override void VisitDelegateDeclaration(DelegateDeclarationSyntax delegateDeclaration)
		{
			// stop visitor for going into delegates to improve performance
		}

		public override void VisitEnumDeclaration(EnumDeclarationSyntax enumDeclaration)
		{
			// stop visitor for going into enums to improve performance
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
		{
			INamedTypeSymbol typeSymbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, _syntaxContext.CancellationToken);
			bool isDacField = typeSymbol?.IsDacField(_pxContext) ?? false;
			ReportMissingXmlCommentsForTypeDeclaration(classDeclaration, doNotReportDiagnostic: isDacField, out bool checkChildNodes, typeSymbol);

			if (!checkChildNodes)
				return;
			
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

			if (!isInsideDacOrDacExt || SystemDacFieldsNames.All.Contains(propertyDeclaration.Identifier.Text))
				return;

			ReportMissingXmlCommentsForTypeMemberDeclaration(propertyDeclaration, propertyDeclaration.Modifiers, 
															 propertyDeclaration.Identifier, doNotReportDiagnostic: false, out _);
		}

		private void ReportMissingXmlCommentsForTypeDeclaration(TypeDeclarationSyntax typeDeclaration, bool doNotReportDiagnostic, out bool checkChildNodes, 
																INamedTypeSymbol typeSymbol = null)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!typeDeclaration.IsPartial())
			{
				ReportMissingXmlCommentsForTypeMemberDeclaration(typeDeclaration, typeDeclaration.Modifiers, typeDeclaration.Identifier, 
																 doNotReportDiagnostic, out checkChildNodes);
				return;
			}

			typeSymbol = typeSymbol ?? _syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, _syntaxContext.CancellationToken);

			if (typeSymbol == null || typeSymbol.DeclaringSyntaxReferences.Length < 2)      //Case when type marked as partial but has only one declaration
			{
				ReportMissingXmlCommentsForTypeMemberDeclaration(typeDeclaration, typeDeclaration.Modifiers, typeDeclaration.Identifier,
																 doNotReportDiagnostic, out checkChildNodes);
				return;
			}
			else if (typeSymbol.DeclaredAccessibility != Accessibility.Public || CheckIfTypeAttributesDisableDiagnostic(typeSymbol))
			{
				checkChildNodes = false;
				return;
			}

			XmlCommentParseResult thisDeclarationParseResult = AnalyzeDeclarationXmlComments(typeDeclaration);
			bool commentsAreValid;
			(commentsAreValid, checkChildNodes) = AnalyzeCommentParseResult(thisDeclarationParseResult);

			if (commentsAreValid)
				return;

			foreach (SyntaxReference reference in typeSymbol.DeclaringSyntaxReferences)
			{
				if (reference.SyntaxTree == typeDeclaration.SyntaxTree ||
					!(reference.GetSyntax(_syntaxContext.CancellationToken) is TypeDeclarationSyntax partialTypeDeclaration))
				{
					continue;
				}

				XmlCommentParseResult parseResult = AnalyzeDeclarationXmlComments(partialTypeDeclaration);
				(commentsAreValid, checkChildNodes) = AnalyzeCommentParseResult(parseResult);

				if (commentsAreValid)
					return;
			}

			if (!doNotReportDiagnostic)
			{
				ReportDiagnostic(_syntaxContext, typeDeclaration, typeDeclaration.Identifier.GetLocation(), thisDeclarationParseResult);
			}
		}

		private bool CheckIfTypeAttributesDisableDiagnostic(INamedTypeSymbol typeSymbol)
		{
			var shortAttributeNames = typeSymbol.GetAttributes()
												.Select(attr => GetAttributeShortName(attr.AttributeClass.Name));

			return CheckAttributeNames(shortAttributeNames);
		}

		private void ReportMissingXmlCommentsForTypeMemberDeclaration(MemberDeclarationSyntax memberDeclaration, SyntaxTokenList modifiers,
																	  SyntaxToken identifier, bool doNotReportDiagnostic, out bool checkChildNodes)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!modifiers.Any(SyntaxKind.PublicKeyword) || CheckIfMemberAttributesDisableDiagnostic(memberDeclaration))
			{
				checkChildNodes = false;
				return;
			}

			XmlCommentParseResult thisDeclarationParseResult = AnalyzeDeclarationXmlComments(memberDeclaration);
			bool commentsAreValid;
			(commentsAreValid, checkChildNodes) = AnalyzeCommentParseResult(thisDeclarationParseResult);

			if (commentsAreValid)
				return;

			if (!doNotReportDiagnostic)
			{
				ReportDiagnostic(_syntaxContext, memberDeclaration, identifier.GetLocation(), thisDeclarationParseResult);
			}
		}

		private XmlCommentParseResult AnalyzeDeclarationXmlComments(MemberDeclarationSyntax memberDeclaration)
		{
			if (!memberDeclaration.HasStructuredTrivia)
				return XmlCommentParseResult.NoXmlComment;

			IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(memberDeclaration);
			bool hasXmlComment = false, hasSummaryTag = false, nonEmptySummaryTag = false;

			foreach (DocumentationCommentTriviaSyntax xmlComment in xmlComments)
			{
				hasXmlComment = true;
				var excludeTag = GetXmlExcludeTag(xmlComment);

				if (excludeTag != null)
					return XmlCommentParseResult.HasExcludeTag;
				else if (hasSummaryTag)
					continue;

				XmlElementSyntax summaryTag = GetSummaryTag(xmlComment);

				if (summaryTag == null)
					continue;

				hasSummaryTag = true;
				nonEmptySummaryTag = IsNonEmptySummaryTag(summaryTag);
			}

			if (!hasXmlComment)
				return XmlCommentParseResult.NoXmlComment;
			else if (!hasSummaryTag)
				return XmlCommentParseResult.NoSummaryTag;
			else if (!nonEmptySummaryTag)
				return XmlCommentParseResult.EmptySummaryTag;
			else
				return XmlCommentParseResult.HasNonEmptySummaryTag;

			//-------------------------------------------------Local function--------------------------------------------------------
			bool IsNonEmptySummaryTag(XmlElementSyntax summaryTag)
			{
				var summaryContent = summaryTag.Content;

				if (summaryContent.Count == 0)
					return false;

				foreach (XmlNodeSyntax contentNode in summaryContent)
				{
					var contentString = contentNode.ToFullString();
					if (contentString.IsNullOrWhiteSpace())
						continue;

					var contentHasText = contentString.Split(_xmlCommentSummarySeparators, StringSplitOptions.RemoveEmptyEntries)
													  .Any(CommentContentIsNotEmpty);
					if (contentHasText)
						return true;
				}

				return false;
			}
		}

		private (bool CommentsAreValid, bool CheckChildNodes) AnalyzeCommentParseResult(XmlCommentParseResult parseResult) =>
			parseResult switch
			{
				XmlCommentParseResult.HasExcludeTag => (CommentsAreValid: true, CheckChildNodes: false),
				XmlCommentParseResult.HasNonEmptySummaryTag => (CommentsAreValid: true, CheckChildNodes: true),
				_ => (CommentsAreValid: false, CheckChildNodes: true),
			};

		private bool CheckIfMemberAttributesDisableDiagnostic(MemberDeclarationSyntax member)
		{
			var shortAttributeNames = member.GetAttributes()
											.Select(attr => GetAttributeShortName(attr));

			return CheckAttributeNames(shortAttributeNames);
		}

		private IEnumerable<DocumentationCommentTriviaSyntax> GetXmlComments(MemberDeclarationSyntax member) =>
			member.GetLeadingTrivia()
				  .Select(t => t.GetStructure())
				  .OfType<DocumentationCommentTriviaSyntax>();

		private XmlEmptyElementSyntax GetXmlExcludeTag(DocumentationCommentTriviaSyntax xmlComment) =>
			xmlComment.ChildNodes()
					  .OfType<XmlEmptyElementSyntax>()
					  .FirstOrDefault(s => XmlAnalyzerConstants.XmlCommentExcludeTag.Equals(s.Name?.ToString(), StringComparison.Ordinal));

		private XmlElementSyntax GetSummaryTag(DocumentationCommentTriviaSyntax xmlComment) =>
			xmlComment.ChildNodes()
					  .OfType<XmlElementSyntax>()
					  .FirstOrDefault(n => XmlAnalyzerConstants.XmlCommentSummaryTag.Equals(n.StartTag?.Name?.ToString(), StringComparison.Ordinal));

		private bool CommentContentIsNotEmpty(string content) =>
			!content.IsNullOrEmpty() &&
			 content.Any(char.IsLetterOrDigit);

		private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, MemberDeclarationSyntax memberDeclaration,
									  Location location, XmlCommentParseResult parseResult)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var memberCategory = GetMemberCategory(memberDeclaration);
			var properties = ImmutableDictionary<string, string>.Empty
																.Add(XmlAnalyzerConstants.XmlCommentParseResultKey, parseResult.ToString());
			var noXmlCommentDiagnostic = Diagnostic.Create(Descriptors.PX1007_PublicClassXmlComment, location, properties, memberCategory);

			syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, _codeAnalysisSettings);
		}

		private LocalizableString GetMemberCategory(MemberDeclarationSyntax memberDeclaration) =>
			 memberDeclaration switch
			 {
				 ClassDeclarationSyntax _     => nameof(Resources.PX1007Class).GetLocalized(),
				 PropertyDeclarationSyntax _  => nameof(Resources.PX1007DacProperty).GetLocalized(),
				 StructDeclarationSyntax _    => nameof(Resources.PX1007Struct).GetLocalized(),
				 InterfaceDeclarationSyntax _ => nameof(Resources.PX1007Interface).GetLocalized(),
				 EnumDeclarationSyntax _      => nameof(Resources.PX1007Enum).GetLocalized(),
				 DelegateDeclarationSyntax _  => nameof(Resources.PX1007Delegate).GetLocalized(),
				 _                            => nameof(Resources.PX1007DefaultEntity).GetLocalized(),
			 };

		private bool CheckAttributeNames(IEnumerable<string> attributeNames)
		{
			const string ObsoleteAttributeShortName = "Obsolete";
			const string PXHiddenAttributeShortName = "PXHidden";
			const string PXInternalUseOnlyAttributeShortName = "PXInternalUseOnly";

			return attributeNames.Any(attrName => attrName == ObsoleteAttributeShortName ||
												  attrName == PXHiddenAttributeShortName ||
												  attrName == PXInternalUseOnlyAttributeShortName);
		}

		private static string GetAttributeShortName(AttributeSyntax attribute)
		{
			string shortName = attribute.Name is QualifiedNameSyntax qualifiedName
				? qualifiedName.Right.ToString()
				: attribute.Name.ToString();

			return GetAttributeShortName(shortName);
		}

		private static string GetAttributeShortName(string attributeName)
		{
			const string AttributeSuffix = "Attribute";
			const int minLengthWithSuffix = 17;

			// perfomance optimization to avoid checking the suffix of attribute names 
			// which are definitely shorter than any of the attributes we search with "Attribute" suffix
			if (attributeName.Length >= minLengthWithSuffix && attributeName.EndsWith(AttributeSuffix))
			{
				const int suffixLength = 9;
				return attributeName.Substring(0, attributeName.Length - suffixLength);
			}

			return attributeName;
		}
	}
}
