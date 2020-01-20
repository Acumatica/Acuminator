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

				if (!CheckXmlCommentAndTheNeedToGoToChildrenNodesForType(classDeclaration, typeSymbol))
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

			if (!isInsideDacOrDacExt || SystemDacFieldsNames.All.Contains(propertyDeclaration.Identifier.Text))
				return;

			CheckXmlCommentAndTheNeedToGoToChildrenNodes(propertyDeclaration, propertyDeclaration.Modifiers, propertyDeclaration.Identifier);
		}

		public override void VisitStructDeclaration(StructDeclarationSyntax structDeclaration)
		{
			if (CheckXmlCommentAndTheNeedToGoToChildrenNodesForType(structDeclaration))
			{
				base.VisitStructDeclaration(structDeclaration);
			}
		}

		public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDeclaration)
		{
			if (CheckXmlCommentAndTheNeedToGoToChildrenNodesForType(interfaceDeclaration))
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

		private bool CheckXmlCommentAndTheNeedToGoToChildrenNodesForType(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol = null)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();
			bool isPartial = typeDeclaration.IsPartial();

			if (!CheckIfTypeIsPublic(typeDeclaration, ref typeSymbol, isPartial) || CheckIfMemberAttributesDisableDiagnostic(typeDeclaration))
				return false;

			if (!typeDeclaration.HasStructuredTrivia)
			{
				ReportDiagnostic(_syntaxContext, typeDeclaration, typeDeclaration.Identifier.GetLocation(), FixOption.NoXmlComment);
				return true;
			}

			IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(typeDeclaration);
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
				ReportDiagnostic(_syntaxContext, typeDeclaration, typeDeclaration.Identifier.GetLocation(), FixOption.NoXmlComment);
			}
			else if (!hasSummaryTag)
			{
				ReportDiagnostic(_syntaxContext, typeDeclaration, typeDeclaration.Identifier.GetLocation(), FixOption.NoSummaryTag);
			}
			else if (!nonEmptySummaryTag)
			{
				ReportDiagnostic(_syntaxContext, typeDeclaration, typeDeclaration.Identifier.GetLocation(), FixOption.EmptySummaryTag);
			}

			return true;
		}

		private bool CheckIfTypeIsPublic(TypeDeclarationSyntax typeDeclaration, ref INamedTypeSymbol typeSymbol, bool isPartial)
		{
			if (typeDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword))
				return true;

			if (isPartial)
			{
				typeSymbol = typeSymbol ?? _syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, _syntaxContext.CancellationToken);
				return typeSymbol?.DeclaredAccessibility == Accessibility.Public;
			}
			else
				return false;
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

		private bool CheckIfMemberAttributesDisableDiagnostic(MemberDeclarationSyntax member) =>
				   member.GetAttributes()
						 .Select(attr => GetAttributeShortName(attr))
						;

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
