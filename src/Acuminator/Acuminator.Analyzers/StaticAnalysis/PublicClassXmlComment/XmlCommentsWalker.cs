﻿using System;
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
			bool isDacOrDacExtension = typeSymbol?.IsDacOrExtension(_pxContext) ?? false;
			AnalyzeTypeDeclarationForMissingXmlComments(classDeclaration, reportDiagnostic: isDacOrDacExtension, out bool checkChildNodes, typeSymbol);

			if (!checkChildNodes)
				return;
			
			try
			{
				_isInsideDacContextStack.Push(isDacOrDacExtension);
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

			AnalyzeTypeMemberDeclarationForMissingXmlComments(propertyDeclaration, propertyDeclaration.Modifiers, 
															 propertyDeclaration.Identifier, reportDiagnostic: true, out _);
		}

		private void AnalyzeTypeDeclarationForMissingXmlComments(TypeDeclarationSyntax typeDeclaration, bool reportDiagnostic, out bool checkChildNodes, 
																 INamedTypeSymbol typeSymbol = null)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!typeDeclaration.IsPartial())
			{
				AnalyzeTypeMemberDeclarationForMissingXmlComments(typeDeclaration, typeDeclaration.Modifiers, typeDeclaration.Identifier,
																 reportDiagnostic, out checkChildNodes);
				return;
			}

			typeSymbol = typeSymbol ?? _syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, _syntaxContext.CancellationToken);

			if (typeSymbol == null || typeSymbol.DeclaringSyntaxReferences.Length < 2)      //Case when type marked as partial but has only one declaration
			{
				AnalyzeTypeMemberDeclarationForMissingXmlComments(typeDeclaration, typeDeclaration.Modifiers, typeDeclaration.Identifier,
																  reportDiagnostic, out checkChildNodes);
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

			if (reportDiagnostic)
			{
				ReportDiagnostic(_syntaxContext, typeDeclaration.Identifier.GetLocation(), thisDeclarationParseResult);
			}
		}

		private bool CheckIfTypeAttributesDisableDiagnostic(INamedTypeSymbol typeSymbol)
		{
			var shortAttributeNames = typeSymbol.GetAttributes()
												.Select(attr => GetAttributeShortName(attr.AttributeClass.Name));

			return CheckAttributeNames(shortAttributeNames);
		}

		private void AnalyzeTypeMemberDeclarationForMissingXmlComments(MemberDeclarationSyntax memberDeclaration, SyntaxTokenList modifiers,
																	   SyntaxToken identifier, bool reportDiagnostic, out bool checkChildNodes)
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

			if (reportDiagnostic)
			{
				ReportDiagnostic(_syntaxContext, identifier.GetLocation(), thisDeclarationParseResult);
			}
		}

		private bool CheckIfMemberAttributesDisableDiagnostic(MemberDeclarationSyntax member)
		{
			var shortAttributeNames = member.GetAttributes()
											.Select(attr => GetAttributeShortName(attr));

			return CheckAttributeNames(shortAttributeNames);
		}
		}

		private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, Location location, XmlCommentParseResult parseResult)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var properties = ImmutableDictionary<string, string>.Empty
																.Add(XmlAnalyzerConstants.XmlCommentParseResultKey, parseResult.ToString());
			var noXmlCommentDiagnostic = Diagnostic.Create(Descriptors.PX1007_PublicClassXmlComment, location, properties);

			syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, _codeAnalysisSettings);
		}

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
