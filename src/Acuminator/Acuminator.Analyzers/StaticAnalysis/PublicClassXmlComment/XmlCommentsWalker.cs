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
	internal class XmlCommentsWalker : CSharpSyntaxWalker
	{
		private static readonly HashSet<string> _systemDacPropertiesExcludedFromDocumentation =
			DacFieldNames.System.All
								.ConcatStructList(DacFieldNames.Restricted.All)
								.AppendItem(DacFieldNames.WellKnown.Selected)
								.ToHashSet(StringComparer.OrdinalIgnoreCase);

		private readonly PXContext _pxContext;
		private readonly SyntaxNodeAnalysisContext _syntaxContext;
		private readonly CodeAnalysisSettings _codeAnalysisSettings;
		private readonly Stack<TypeInfo> _containingTypesStack = new(capacity: 2);

		private readonly ExcludeFromDocsAttributesChecker _attributesChecker = new();
		private readonly XmlCommentsParser _xmlCommentsParser;
		private readonly MappedDacPropertyFinder _mappedDacPropertyFinder;

		public XmlCommentsWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
								 CodeAnalysisSettings codeAnalysisSettings)
		{
			_syntaxContext = syntaxContext;
			_pxContext = pxContext;
			_codeAnalysisSettings = codeAnalysisSettings;
			_xmlCommentsParser = new XmlCommentsParser(syntaxContext.SemanticModel, syntaxContext.CancellationToken);
			_mappedDacPropertyFinder = new MappedDacPropertyFinder(pxContext, syntaxContext.SemanticModel, syntaxContext.CancellationToken);
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
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!typeDeclaration.IsPartial() ||
				_syntaxContext.SemanticModel.GetDeclaredSymbol(typeDeclaration, _syntaxContext.CancellationToken) is not INamedTypeSymbol partialTypeSymbol)
			{
				if (!typeDeclaration.IsPublic())
					return;

				var attributesLists = typeDeclaration.AttributeLists;

				if (attributesLists.Count > 0 &&
					_attributesChecker.CheckIfAttributesDisableDiagnostic(typeDeclaration, checkForPXHidden: false))
				{
					return;
				}
			}
			else if (partialTypeSymbol.DeclaredAccessibility != Accessibility.Public ||
					 _attributesChecker.CheckIfAttributesDisableDiagnostic(partialTypeSymbol, checkForPXHidden: false))
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

			bool hasPublicAccessibility = classDeclaration.IsPublic();

			if (!classDeclaration.IsPartial() && !hasPublicAccessibility)
				return;

			INamedTypeSymbol? typeSymbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, _syntaxContext.CancellationToken);

			if ((typeSymbol == null && !hasPublicAccessibility) ||
				 typeSymbol != null && typeSymbol?.DeclaredAccessibility != Accessibility.Public)
			{
				return;
			}

			var containingTypeInfo = new TypeInfo(typeSymbol, _pxContext);

			if (CheckIfTypeIsExcludedFromDocumentation(containingTypeInfo, classDeclaration))
				return;

			AnalyzeTypeDeclarationForMissingXmlComments(classDeclaration, containingTypeInfo, out bool stepIntoChildren);

			if (!stepIntoChildren)
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
				return _attributesChecker.CheckIfAttributesDisableDiagnostic(typeInfo.ContainingType,
																			 checkForPXHidden: typeInfo.DacKind == DacType.Dac);
			}
			else
			{
				return _attributesChecker.CheckIfAttributesDisableDiagnostic(classDeclaration, checkForPXHidden: false);
			}
		}

		private void AnalyzeTypeDeclarationForMissingXmlComments(TypeDeclarationSyntax typeDeclaration, TypeInfo typeInfo, out bool stepIntoChildren)
		{
			bool hasMultipleDeclarations = typeDeclaration.IsPartial() && typeInfo.ContainingType?.DeclaringSyntaxReferences.Length > 1;

			if (hasMultipleDeclarations)
				AnalyzeSingleTypeDeclaration(typeDeclaration, typeInfo, out stepIntoChildren);
			else
				AnalizeMultipleTypeDeclarations(typeDeclaration, typeInfo, out stepIntoChildren);
		}

		private void AnalizeMultipleTypeDeclarations(TypeDeclarationSyntax typeDeclaration, TypeInfo typeInfo, out bool stepIntoChildren)
		{
			if (typeInfo.ContainingType == null || typeInfo.ContainingType.DeclaringSyntaxReferences.Length < 2)
			{
				AnalyzeSingleTypeDeclaration(typeDeclaration, typeInfo, out stepIntoChildren);
				return;
			}

			// First, look for the comments in the declaration being analyzed because we already have its syntax node
			XmlCommentsParseInfo thisDeclarationCommentsParseInfo = _xmlCommentsParser.AnalyzeXmlComments(typeDeclaration, mappedDacProperty: null);

			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			// If the parser found some XML comments then we use this result and don't check other declarations
			if (thisDeclarationCommentsParseInfo.ParseResult != XmlCommentParseResult.NoXmlComment)
			{
				var locationToReport = typeDeclaration.Identifier.GetLocation();
				AnalyzeSingleTypeDeclaration(thisDeclarationCommentsParseInfo, locationToReport, typeInfo, out stepIntoChildren);
				return;
			}

			List<Location>? partialDeclarationIdentifiersLocations = null;

			// Check other partial type declarations
			foreach (SyntaxReference reference in typeInfo.ContainingType.DeclaringSyntaxReferences)
			{
				_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

				if (reference.SyntaxTree == typeDeclaration.SyntaxTree ||
					reference.GetSyntax(_syntaxContext.CancellationToken) is not TypeDeclarationSyntax partialTypeDeclaration)
				{
					continue;
				}
				
				XmlCommentsParseInfo partialDeclarationParseInfo = _xmlCommentsParser.AnalyzeXmlComments(partialTypeDeclaration, mappedDacProperty: null);

				if (partialDeclarationParseInfo.ParseResult != XmlCommentParseResult.NoXmlComment)
				{
					var locationToReport = typeDeclaration.Identifier.GetLocation();
					AnalyzeSingleTypeDeclaration(partialDeclarationParseInfo, locationToReport, typeInfo, out stepIntoChildren);
					return;
				}

				var partialDeclarationIdentifierLocation = partialTypeDeclaration.Identifier.GetLocation();

				if (partialDeclarationIdentifierLocation != null)
				{
					partialDeclarationIdentifiersLocations ??= new List<Location>(capacity: 4);
					partialDeclarationIdentifiersLocations.Add(partialDeclarationIdentifierLocation);
				}
			}

			// Reaching this part of code means there are no XML doc comments on any partial type declaration
			stepIntoChildren = true;

			if (typeInfo.IsDacOrDacExtension)
			{
				ReportDiagnostic(_syntaxContext, Descriptors.PX1007_PublicClassNoXmlComment, typeDeclaration.Identifier.GetLocation(),
								 XmlCommentParseResult.NoXmlComment, extraLocations: partialDeclarationIdentifiersLocations, mappedOriginalDacProperty: null);
			}
		}

		private void AnalyzeSingleTypeDeclaration(TypeDeclarationSyntax typeDeclaration, TypeInfo typeInfo, out bool stepIntoChildren)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			XmlCommentsParseInfo typeCommentsParseInfo = _xmlCommentsParser.AnalyzeXmlComments(typeDeclaration, mappedDacProperty: null);
			AnalyzeSingleTypeDeclaration(typeCommentsParseInfo, typeDeclaration.Identifier.GetLocation(), typeInfo, out stepIntoChildren);
		}

		private void AnalyzeSingleTypeDeclaration(XmlCommentsParseInfo typeCommentsParseInfo, Location primaryLocationToReport, TypeInfo typeInfo,
												  out bool stepIntoChildren)
		{
			stepIntoChildren = typeCommentsParseInfo.StepIntoChildren;

			if (typeCommentsParseInfo.HasError && typeInfo.IsDacOrDacExtension)
			{
				var extraLocations = typeCommentsParseInfo.DocCommentLocationsWithErrors;

				ReportDiagnostic(_syntaxContext, typeCommentsParseInfo.DiagnosticToReport, primaryLocationToReport, 
								 typeCommentsParseInfo.ParseResult, extraLocations, mappedOriginalDacProperty: null);
			}
		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclaration)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!propertyDeclaration.IsPublic())
				return;

			var containingTypeInfo = _containingTypesStack.Count > 0
				? _containingTypesStack.Peek()
				: null;

			bool isInsideDacOrDacExt = containingTypeInfo?.IsDacOrDacExtension ?? false;

			if (!isInsideDacOrDacExt)
				return;

			if (_systemDacPropertiesExcludedFromDocumentation.Contains(propertyDeclaration.Identifier.Text))
				return;

			AnalyzePropertyDeclarationXmlComments(propertyDeclaration, containingTypeInfo!);
		}

		private void AnalyzePropertyDeclarationXmlComments(PropertyDeclarationSyntax propertyDeclaration, TypeInfo containingTypeInfo)
		{
			_syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (_attributesChecker.CheckIfAttributesDisableDiagnostic(propertyDeclaration, checkForPXHidden: false))
				return;

			IPropertySymbol? mappedOriginalDacProperty	   = _mappedDacPropertyFinder.GetMappedPropertyFromTheOriginalDac(containingTypeInfo, propertyDeclaration);
			XmlCommentsParseInfo propertyCommentsParseInfo = _xmlCommentsParser.AnalyzeXmlComments(propertyDeclaration, mappedOriginalDacProperty);
			
			if (propertyCommentsParseInfo.HasError)
			{
				ReportDiagnostic(_syntaxContext, propertyCommentsParseInfo.DiagnosticToReport, propertyDeclaration.Identifier.GetLocation(), 
								 propertyCommentsParseInfo.ParseResult, propertyCommentsParseInfo.DocCommentLocationsWithErrors, mappedOriginalDacProperty);
			}
		}

		private void ReportDiagnostic(SyntaxNodeAnalysisContext syntaxContext, DiagnosticDescriptor diagnosticDescriptor, Location primaryLocation, 
									  XmlCommentParseResult parseResult, IEnumerable<Location>? extraLocations, IPropertySymbol? mappedOriginalDacProperty)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			bool isDiagnosticForProjectionDacProperty = mappedOriginalDacProperty != null;
			var properties = new Dictionary<string, string>
			{
				{ DocumentationDiagnosticProperties.ParseResult, parseResult.ToString() },
				{ DocumentationDiagnosticProperties.IsProjectionDacProperty, isDiagnosticForProjectionDacProperty.ToString() }
			};

			if (isDiagnosticForProjectionDacProperty)
			{
				var mappedBqlField = mappedOriginalDacProperty!.GetCorrespondingBqlField(_pxContext, checkContainingTypeIsDac: false);

				if (mappedBqlField != null)
					properties.Add(DocumentationDiagnosticProperties.MappedDacBqlFieldMetadataName, mappedBqlField.GetCLRTypeNameFromType());
			}

			var diagnostic = extraLocations.IsNullOrEmpty()
				? Diagnostic.Create(diagnosticDescriptor, primaryLocation, properties.ToImmutableDictionary())
				: Diagnostic.Create(diagnosticDescriptor, primaryLocation, extraLocations, properties.ToImmutableDictionary());

			syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _codeAnalysisSettings);
		}
	}
}
