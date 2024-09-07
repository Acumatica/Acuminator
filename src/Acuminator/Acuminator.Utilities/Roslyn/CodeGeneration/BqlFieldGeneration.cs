using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Roslyn utils for BQL Field generation.
	/// </summary>
	public static class BqlFieldGeneration
	{
		public static ClassDeclarationSyntax? GenerateWeaklyTypedBqlField(string bqlFieldName, bool isFirstField,
																		  PropertyDeclarationSyntax? fieldPropertyToCopyRegions)
		{
			var iBqlFieldBaseTypeNode = IBqlFieldBaseTypeForBqlField();
			var bqlField = GenerateBqlField(fieldPropertyToCopyRegions, iBqlFieldBaseTypeNode, bqlFieldName, isFirstField);

			return bqlField;
		}

		public static SimpleBaseTypeSyntax IBqlFieldBaseTypeForBqlField()
		{
			var iBqlFieldBaseType =
				SimpleBaseType(
					QualifiedName(
						QualifiedName(
							IdentifierName("PX"),
							IdentifierName("Data")),
							IdentifierName(TypeNames.IBqlField)));
			return iBqlFieldBaseType;
		}

		public static ClassDeclarationSyntax? GenerateTypedBqlField(PropertyTypeName fieldPropertyTypeName, string bqlFieldName, bool isFirstField,
																	PropertyDeclarationSyntax? fieldPropertyToCopyRegions)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(fieldPropertyTypeName, bqlFieldName);

			if (bqlFieldBaseTypeNode == null)
				return null;

			var bqlField = GenerateBqlField(fieldPropertyToCopyRegions, bqlFieldBaseTypeNode, bqlFieldName, isFirstField);
			return bqlField;
		}

		public static ClassDeclarationSyntax? GenerateTypedBqlField(BqlFieldTypeName bqlFieldTypeName, string bqlFieldName, bool isFirstField,
																	PropertyDeclarationSyntax? fieldPropertyToCopyRegions)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(bqlFieldTypeName, bqlFieldName);
			var bqlField = GenerateBqlField(fieldPropertyToCopyRegions, bqlFieldBaseTypeNode, bqlFieldName, isFirstField);

			return bqlField;
		}

		public static SimpleBaseTypeSyntax? BaseTypeForBqlField(PropertyTypeName fieldPropertyTypeName, string bqlFieldName)
		{
			bqlFieldName.ThrowOnNullOrWhiteSpace();

			var bqlFieldTypeName = PropertyTypeToBqlFieldTypeMapping.GetBqlFieldType(fieldPropertyTypeName).NullIfWhiteSpace();

			if (bqlFieldTypeName == null)
				return null;

			var bqlFieldType = BaseTypeForBqlField(bqlFieldTypeName, bqlFieldName);
			return bqlFieldType;
		}

		public static SimpleBaseTypeSyntax BaseTypeForBqlField(BqlFieldTypeName bqlFieldTypeName, string bqlFieldName)
		{
			bqlFieldName.ThrowOnNullOrWhiteSpace();

			var bqlFieldType = BaseTypeForBqlField(bqlFieldTypeName, bqlFieldName);
			return bqlFieldType;
		}

		private static SimpleBaseTypeSyntax BaseTypeForBqlField(string bqlFieldTypeName, string bqlFieldName)
		{
			GenericNameSyntax fieldTypeNode =
				GenericName(Identifier("Field"))
					.WithTypeArgumentList(
						TypeArgumentList(
							SingletonSeparatedList<TypeSyntax>(IdentifierName(bqlFieldName)))
						.WithGreaterThanToken(
							Token(leading: TriviaList(), SyntaxKind.GreaterThanToken, TriviaList(Space))));

			var newBaseType =
				SimpleBaseType(
					QualifiedName(
						QualifiedName(
							QualifiedName(
								QualifiedName(
									IdentifierName("PX"),
									IdentifierName("Data")),
									IdentifierName("BQL")),
									IdentifierName(bqlFieldTypeName)),
						fieldTypeNode));

			return newBaseType;
		}

		private static ClassDeclarationSyntax GenerateBqlField(PropertyDeclarationSyntax? property, SimpleBaseTypeSyntax bqlFieldBaseType,
															   string bqlFieldName, bool isFirstField)
		{
			var baseTypesListNode = BaseList(
										SingletonSeparatedList<BaseTypeSyntax>(bqlFieldBaseType));
			var bqlFieldNode = ClassDeclaration(
									attributeLists: default,
									TokenList(
										Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)),
									Identifier(bqlFieldName), typeParameterList: null, baseTypesListNode,
									constraintClauses: default, members: default)
								.WithOpenBraceToken(
									Token(leading: TriviaList(), SyntaxKind.OpenBraceToken, TriviaList()));

			var clostBracketToken = isFirstField
				? Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken,
						TriviaList(CarriageReturn, LineFeed, CarriageReturn, LineFeed))
				: Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken, TriviaList(CarriageReturn, LineFeed));

			bqlFieldNode = bqlFieldNode.WithCloseBraceToken(clostBracketToken);

			if (property != null)
				bqlFieldNode = CopyRegionsFromProperty(bqlFieldNode, property);

			return bqlFieldNode;
		}

		private static ClassDeclarationSyntax CopyRegionsFromProperty(ClassDeclarationSyntax bqlFieldNode, PropertyDeclarationSyntax property)
		{
			var leadingTrivia = property.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return bqlFieldNode;

			var regionTrivias = leadingTrivia.GetRegionDirectiveLinesFromTrivia();

			if (regionTrivias.Count == 0)
				return bqlFieldNode;

			var regionsTrivia = TriviaList(regionTrivias);
			var bqlFieldNodeLeadingTrivia = bqlFieldNode.GetLeadingTrivia();
			var newBqlFieldNodeTrivia = bqlFieldNodeLeadingTrivia.AddRange(regionsTrivia);

			return bqlFieldNode.WithLeadingTrivia(newBqlFieldNodeTrivia);
		}
	}
}
