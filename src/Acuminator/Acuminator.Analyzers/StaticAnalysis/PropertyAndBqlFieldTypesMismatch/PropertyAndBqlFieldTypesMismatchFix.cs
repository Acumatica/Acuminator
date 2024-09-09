using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.CodeGeneration;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PropertyAndBqlFieldTypesMismatch
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class PropertyAndBqlFieldTypesMismatchFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.PropertyType, out string? propertyDataTypeName) ||
				!diagnostic.TryGetPropertyValue(DiagnosticProperty.BqlFieldDataType, out string? bqlFieldDataTypeName) ||
				!diagnostic.TryGetPropertyValue(DiagnosticProperty.BqlFieldName, out string? bqlFieldName) ||
				!diagnostic.TryGetPropertyValue(PX1068DiagnosticProperty.MakePropertyTypeNullable, out string? makePropertyTypeNullableStr) ||
				!bool.TryParse(makePropertyTypeNullableStr, out bool makePropertyTypeNullable))
			{
				return;
			}

			bqlFieldDataTypeName = bqlFieldDataTypeName.NullIfWhiteSpace();
			propertyDataTypeName = propertyDataTypeName.NullIfWhiteSpace();
			bqlFieldName		 = bqlFieldName.NullIfWhiteSpace();

			bool isOnPropertyNode = diagnostic.IsFlagSet(DiagnosticProperty.IsProperty);

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var nodeWithDiagnostic = root?.FindNode(context.Span);

			if (nodeWithDiagnostic == null)
				return;

			if (isOnPropertyNode)
				RegisterFixPropertyTypeAction(context, diagnostic, root!, nodeWithDiagnostic, bqlFieldDataTypeName, makePropertyTypeNullable);
			else
				RegisterFixBqlFieldTypeAction(context, diagnostic, root!, nodeWithDiagnostic, propertyDataTypeName, bqlFieldName);
		}

		private void RegisterFixPropertyTypeAction(CodeFixContext context, Diagnostic diagnostic, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
												   string? bqlFieldDataTypeName, bool makePropertyTypeNullable)
		{
			var propertyNode = nodeWithDiagnostic.ParentOrSelf<PropertyDeclarationSyntax>();

			if (propertyNode == null || bqlFieldDataTypeName.IsNullOrWhiteSpace()) 
				return;

			string codeActionName = nameof(Resources.PX1068FixPropertyType).GetLocalized().ToString();
			Document document 	  = context.Document;

			var codeAction = CodeAction.Create(codeActionName,
											   cToken => ChangePropertyTypeToBqlFieldTypeAsync(document, root, propertyNode, bqlFieldDataTypeName, 
																							   makePropertyTypeNullable, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
		}

		private void RegisterFixBqlFieldTypeAction(CodeFixContext context, Diagnostic diagnostic, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
												   string? propertyDataTypeName, string? bqlFieldName)
		{
			if (bqlFieldName == null)
				return;

			string? bqlFieldTypeName = GetBqlFieldTypeFromPropertyDataType(propertyDataTypeName);
			
			if (bqlFieldTypeName == null)
				return;

			string codeActionName = nameof(Resources.PX1068FixBqlType).GetLocalized().ToString();
			Document document	  = context.Document;

			var codeAction = CodeAction.Create(codeActionName,
											   cToken => ChangeBqlFieldTypeToPropertyTypeAsync(document, root, nodeWithDiagnostic, 
																								bqlFieldTypeName, bqlFieldName, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
		}

		private string? GetBqlFieldTypeFromPropertyDataType(string? propertyDataTypeName)
		{
			if (propertyDataTypeName.IsNullOrWhiteSpace())
				return null;

			var dataTypeName = new DataTypeName(propertyDataTypeName);
			string? mappedBqlFieldType = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(dataTypeName);

			return mappedBqlFieldType;
		}

		private Task<Document> ChangePropertyTypeToBqlFieldTypeAsync(Document document, SyntaxNode root, PropertyDeclarationSyntax propertyNode,
																	 string newPropertyDataTypeName, bool makePropertyTypeNullable,
																	 CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var newPropertyType = CreateDataTypeNode(newPropertyDataTypeName, makePropertyTypeNullable);
			var newPropertyNode = propertyNode.WithType(newPropertyType);
			var newRoot			= root.ReplaceNode(propertyNode, newPropertyNode);

			cancellation.ThrowIfCancellationRequested();

			var newDocument = document.WithSyntaxRoot(newRoot);
			return Task.FromResult(newDocument);
		}

		private TypeSyntax CreateDataTypeNode(string dataTypeName, bool makePropertyTypeNullable)
		{			
			int indexOfOpeningSquareBracket = dataTypeName.LastIndexOf('[');
			bool isArrayType = indexOfOpeningSquareBracket >= 0;

			if (isArrayType)
			{
				var elementTypeName	   = dataTypeName[..indexOfOpeningSquareBracket].Trim();
				var predefinedTypeKind = GetPredefinedTypeKind(elementTypeName);

				TypeSyntax elementTypeNode = predefinedTypeKind.HasValue
					? PredefinedType(
						Token(predefinedTypeKind.Value))
					: IdentifierName(elementTypeName); 
				
				var arrayRankSpecifier = ArrayRankSpecifier(
											SingletonSeparatedList<ExpressionSyntax>(
												OmittedArraySizeExpression()));
				var arrayType = ArrayType(elementTypeNode,
										  SingletonList(arrayRankSpecifier));
				return arrayType;
			}
			else
			{
				var predefinedTypeKind  = GetPredefinedTypeKind(dataTypeName);
				TypeSyntax dataTypeNode = predefinedTypeKind.HasValue
					? PredefinedType(
						Token(predefinedTypeKind.Value))
					: IdentifierName(dataTypeName);

				return makePropertyTypeNullable
					? NullableType(dataTypeNode)
					: dataTypeNode;
			}	
		}

		private SyntaxKind? GetPredefinedTypeKind(string dataTypeName)
		{
			SyntaxKind keywordKind = SyntaxFacts.GetKeywordKind(dataTypeName);
			return SyntaxFacts.IsPredefinedType(keywordKind)
				? keywordKind
				: null;
		}

		private Task<Document> ChangeBqlFieldTypeToPropertyTypeAsync(Document document, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
																	  string newBqlFieldTypeName, string bqlFieldName,
																	  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			bool isInBaseList = nodeWithDiagnostic.ParentOrSelf<BaseListSyntax>() != null;

			if (isInBaseList)
			{
				return ChangePartOfBqlFieldTypeAsync(document, root, nodeWithDiagnostic, newBqlFieldTypeName, bqlFieldName, 
													 cancellation);
			}
			else
			{
				return ChangeEntireBqlFieldTypeAsync(document, root, nodeWithDiagnostic, newBqlFieldTypeName, bqlFieldName,
													 cancellation);
			}
		}

		private Task<Document> ChangePartOfBqlFieldTypeAsync(Document document, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
															 string newBqlFieldTypeName, string bqlFieldName,
															 CancellationToken cancellation)
		{
			if (nodeWithDiagnostic is not IdentifierNameSyntax bqlFieldTypeNode || !IsBqlFieldTypeNode(bqlFieldTypeNode))
			{
				return ChangeEntireBqlFieldTypeAsync(document, root, nodeWithDiagnostic, newBqlFieldTypeName,
													 bqlFieldName, cancellation);
			}

			cancellation.ThrowIfCancellationRequested();

			var newBqlFieldTypeNode = IdentifierName(newBqlFieldTypeName);
			var newRoot				= root.ReplaceNode(bqlFieldTypeNode, newBqlFieldTypeNode);
			var newDocument			= document.WithSyntaxRoot(newRoot);

			return Task.FromResult(newDocument);
		}

		private bool IsBqlFieldTypeNode(IdentifierNameSyntax bqlFieldTypeNode)
		{
			string nodeText = bqlFieldTypeNode.Identifier.Text;

			if (nodeText.IsNullOrWhiteSpace())
				return false;

			var strongBqlFieldTypeName = new BqlFieldTypeName(nodeText);
			return DataTypeToBqlFieldTypeMapping.ContainsBqlFieldType(strongBqlFieldTypeName);
		}

		private Task<Document> ChangeEntireBqlFieldTypeAsync(Document document, SyntaxNode root, SyntaxNode nodeWithDiagnostic,
															 string newBqlFieldTypeName, string bqlFieldName, CancellationToken cancellation)
		{
			var bqlFieldNode = nodeWithDiagnostic.ParentOrSelf<ClassDeclarationSyntax>();

			if (bqlFieldNode == null || !IsBqlFieldDeclaration(bqlFieldNode))
				return Task.FromResult(document);

			var strongBqlFieldTypeName = new BqlFieldTypeName(newBqlFieldTypeName);
			var newBqlFieldBaseType	   = BqlFieldGeneration.BaseTypeForBqlField(strongBqlFieldTypeName, bqlFieldName);

			cancellation.ThrowIfCancellationRequested();

			var newBqlFieldNode =
				bqlFieldNode.WithBaseList(
								BaseList(
									SingletonSeparatedList<BaseTypeSyntax>(newBqlFieldBaseType)));

			var newRoot		= root.ReplaceNode(bqlFieldNode, newBqlFieldNode);
			var newDocument = document.WithSyntaxRoot(newRoot);

			return Task.FromResult(newDocument);
		}

		private bool IsBqlFieldDeclaration(ClassDeclarationSyntax bqlFieldNode)
		{
			bool isPublic 	 = bqlFieldNode.Modifiers.Any(SyntaxKind.PublicKeyword);
			bool isAbstract  = bqlFieldNode.Modifiers.Any(SyntaxKind.AbstractKeyword);
			bool hasBaseType = bqlFieldNode.BaseList != null;

			return isPublic && isAbstract && hasBaseType;
		}
	}
}