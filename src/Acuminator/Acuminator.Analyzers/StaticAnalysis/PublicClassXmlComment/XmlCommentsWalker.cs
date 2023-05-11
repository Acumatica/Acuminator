#nullable enable

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
	internal partial class XmlCommentsWalker : CSharpSyntaxWalker
	{
		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };

		private readonly PXContext _pxContext;
		private readonly SyntaxNodeAnalysisContext _syntaxContext;
		private readonly CodeAnalysisSettings _codeAnalysisSettings;
		private readonly Stack<TypeInfo> _containingTypesStack = new(capacity: 2);

		public XmlCommentsWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
								 CodeAnalysisSettings codeAnalysisSettings)
		{
			_syntaxContext = syntaxContext;
			_pxContext = pxContext;
			_codeAnalysisSettings = codeAnalysisSettings;
		}

		#region Optimization - skipping visit of some subtrees
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

		public override void VisitEventDeclaration(EventDeclarationSyntax node)
		{
			// stop visitor for going into events to improve performance
		}

		public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
		{
			// stop visitor for going into fields to improve performance
		}

		public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
		{
			// stop visitor for going into events to improve performance
		}

		public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
		{
			// stop visitor for going into operators to improve performance
		}

		public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
		{
			// stop visitor for going into operators to improve performance
		}

		public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
		{
			// stop visitor for going into finalyzers to improve performance
		}

		public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
		{
			// stop visitor for going into indexers to improve performance
		}
		#endregion

		public override void VisitStructDeclaration(StructDeclarationSyntax structDeclaration) =>
			VisitNonDacTypeDeclaration(structDeclaration, base.VisitStructDeclaration);

		public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax interfaceDeclaration) =>
			VisitNonDacTypeDeclaration(interfaceDeclaration, base.VisitInterfaceDeclaration);

		private void VisitNonDacTypeDeclaration<TTypeDeclaration>(TTypeDeclaration typeDeclaration, Action<TTypeDeclaration> visitSubtreeAction)
		where TTypeDeclaration : TypeDeclarationSyntax
		{
			var attributesLists = typeDeclaration.AttributeLists;

			if (attributesLists.Count > 0 &&
				_attributesChecker.CheckIfAttributesDisableDiagnostic(typeDeclaration, checkForPXHidden: false))
			{
				return;
			}

			try
			{
				_containingTypesStack.Push(TypeInfo.NonDacTypeInfo);
				visitSubtreeAction(typeDeclaration);
			}
			finally
			{
				_containingTypesStack.Pop();
			}
		}

		public override void VisitClassDeclaration(ClassDeclarationSyntax classDeclaration)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			INamedTypeSymbol? typeSymbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, _syntaxContext.CancellationToken);
			var containingTypeInfo = new TypeInfo(typeSymbol, _pxContext);

			if (CheckIfTypeIsExcludedFromDocumentation(containingTypeInfo, classDeclaration))
				return;

			AnalyzeTypeDeclarationForMissingXmlComments(classDeclaration, containingTypeInfo, out bool checkChildNodes);

			if (!checkChildNodes)
				return;

			try
			{
				_containingTypesStack.Push(containingTypeInfo);
				base.VisitClassDeclaration(classDeclaration);
			}
			finally
			{
				_containingTypesStack.Pop();
			}
		}

		private bool CheckIfTypeIsExcludedFromDocumentation(TypeInfo typeInfo, ClassDeclarationSyntax classDeclaration)
		{
			if (typeInfo.ContainingType != null)
			{
				if (typeInfo.ContainingType.DeclaredAccessibility != Accessibility.Public)
					return false;

				return _attributesChecker.CheckIfAttributesDisableDiagnostic(typeInfo.ContainingType,
																			 checkForPXHidden: typeInfo.DacKind == DacType.Dac);
			}
			else
			{
				if (!classDeclaration.IsPublic())
					return false;

				return _attributesChecker.CheckIfAttributesDisableDiagnostic(classDeclaration, checkForPXHidden: false);
			}
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


		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
		{
			var containingTypeInfo = _containingTypesStack.Count > 0
				? _containingTypesStack.Peek()
				: null;

			bool isInsideDacOrDacExt = containingTypeInfo?.IsDacOrDacExtension ?? false;

			if (!isInsideDacOrDacExt)
				return;

			string propertyName = propertyDeclaration.Identifier.Text;

			if (DacFieldNames.System.All.Contains(propertyName) || DacFieldNames.WellKnown.Selected.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
				return;

			AnalyzePropertyDeclarationForMissingXmlComments(propertyDeclaration, propertyDeclaration.Modifiers,
															 propertyDeclaration.Identifier, reportDiagnostic: true, out _);
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

		private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, Location location, XmlCommentParseResult parseResult)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var properties = ImmutableDictionary<string, string>.Empty
																.Add(XmlAnalyzerConstants.XmlCommentParseResultKey, parseResult.ToString());
			var noXmlCommentDiagnostic = Diagnostic.Create(Descriptors.PX1007_PublicClassXmlComment, location, properties);

			syntaxContext.ReportDiagnosticWithSuppressionCheck(noXmlCommentDiagnostic, _codeAnalysisSettings);
		}

		

		private ImmutableArray<Location> GetErrorLocationsFromTagNodesWithFallbackToMemberDeclaration(List<XmlCommentTagsInfo> tagsInfos,
																									  MemberDeclarationSyntax memberDeclaration)
		{
			var errorLocations = tagsInfos.SelectMany(tagInfo => tagInfo.GetAllTagNodes())
										  .Select(tagNode => tagNode.GetLocation())
										  .Where(location => location != null)
										  .ToImmutableArray();

			return errorLocations.IsDefaultOrEmpty
				? GetErrorLocationsFromMemberDeclaration(memberDeclaration)
				: errorLocations;
		}

		private ImmutableArray<Location> GetErrorLocationsFromMemberDeclaration(MemberDeclarationSyntax memberDeclaration)
		{
			var errorLocations = memberDeclaration.GetIdentifiers()
												  .Select(identifier => identifier.GetLocation())
												  .Where(location => location != null)
												  .ToImmutableArray();
			if (!errorLocations.IsDefaultOrEmpty)
				return errorLocations;

			var memberDeclarationLocation = memberDeclaration.GetLocation();

			return memberDeclarationLocation != null
				? ImmutableArray.Create(memberDeclarationLocation)
				: ImmutableArray<Location>.Empty;
		}
	}
}
