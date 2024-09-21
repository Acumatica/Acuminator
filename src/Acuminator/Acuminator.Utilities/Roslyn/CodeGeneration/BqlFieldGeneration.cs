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
		public static ClassDeclarationSyntax? GenerateWeaklyTypedBqlField(string bqlFieldName, bool isFirstField, bool isRedeclaration,
																		  MemberDeclarationSyntax? adjacentMemberToCopyRegions)
		{
			var iBqlFieldBaseTypeNode = IBqlFieldBaseTypeForBqlField();
			var bqlField = GenerateBqlField(adjacentMemberToCopyRegions, iBqlFieldBaseTypeNode, bqlFieldName, isFirstField, isRedeclaration);

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
							IdentifierName(TypeNames.BqlField.IBqlField)));
			return iBqlFieldBaseType;
		}

		public static ClassDeclarationSyntax? GenerateTypedBqlField(DataTypeName dataTypeName, string bqlFieldName, bool isFirstField,
																	bool isRedeclaration, MemberDeclarationSyntax? adjacentMemberToCopyRegions)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(dataTypeName, bqlFieldName);

			if (bqlFieldBaseTypeNode == null)
				return null;

			var bqlField = GenerateBqlField(adjacentMemberToCopyRegions, bqlFieldBaseTypeNode, bqlFieldName, isFirstField, isRedeclaration);
			return bqlField;
		}

		public static ClassDeclarationSyntax? GenerateTypedBqlField(BqlFieldTypeName bqlFieldTypeName, string bqlFieldName, bool isFirstField,
																	bool isRedeclaration, MemberDeclarationSyntax? adjacentMemberToCopyRegions)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(bqlFieldTypeName, bqlFieldName);
			var bqlField = GenerateBqlField(adjacentMemberToCopyRegions, bqlFieldBaseTypeNode, bqlFieldName, isFirstField, isRedeclaration);

			return bqlField;
		}

		public static SimpleBaseTypeSyntax? BaseTypeForBqlField(DataTypeName dataTypeName, string bqlFieldName)
		{
			bqlFieldName.ThrowOnNullOrWhiteSpace();

			var bqlFieldTypeName = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(dataTypeName).NullIfWhiteSpace();

			if (bqlFieldTypeName == null)
				return null;

			var bqlFieldType = BaseTypeForBqlFieldImpl(bqlFieldTypeName, bqlFieldName);
			return bqlFieldType;
		}

		public static SimpleBaseTypeSyntax BaseTypeForBqlField(BqlFieldTypeName bqlFieldTypeName, string bqlFieldName)
		{
			bqlFieldName.ThrowOnNullOrWhiteSpace();

			var bqlFieldType = BaseTypeForBqlFieldImpl(bqlFieldTypeName.Value, bqlFieldName);
			return bqlFieldType;
		}

		private static SimpleBaseTypeSyntax BaseTypeForBqlFieldImpl(string bqlFieldTypeName, string bqlFieldName)
		{
			GenericNameSyntax fieldTypeNode =
				GenericName(Identifier("Field"))
					.WithTypeArgumentList(
						TypeArgumentList(
							SingletonSeparatedList<TypeSyntax>(IdentifierName(bqlFieldName)))
						.WithGreaterThanToken(
							Token(leading: TriviaList(), SyntaxKind.GreaterThanToken, TriviaList(Space))));

			bool isAttributesBqlField = bqlFieldName.Equals(DacFieldNames.System.Attributes, StringComparison.OrdinalIgnoreCase); 
			QualifiedNameSyntax bqlFieldNamespaceName;

			if (isAttributesBqlField)
			{
				bqlFieldNamespaceName =
					QualifiedName(
						QualifiedName(
							IdentifierName("PX"),
							IdentifierName("Objects")),
							IdentifierName("CR"));
			}
			else
			{
				bqlFieldNamespaceName =
					QualifiedName(
						QualifiedName(
							IdentifierName("PX"),
							IdentifierName("Data")),
							IdentifierName("BQL"));
			}

			var newBaseType =
				SimpleBaseType(
					QualifiedName(
						QualifiedName(
							bqlFieldNamespaceName,
							IdentifierName(bqlFieldTypeName)),
						fieldTypeNode));

			return newBaseType;
		}

		private static ClassDeclarationSyntax GenerateBqlField(MemberDeclarationSyntax? adjacentMemberToCopyRegions, 
																SimpleBaseTypeSyntax bqlFieldBaseType, string bqlFieldName, bool isFirstField, 
																bool isRedeclaration)
		{
			var baseTypesListNode = BaseList(
										SingletonSeparatedList<BaseTypeSyntax>(bqlFieldBaseType));
			SyntaxTokenList modifiers = isRedeclaration
				? TokenList(
					Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.NewKeyword), Token(SyntaxKind.AbstractKeyword))
				: TokenList(
					Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword));

			var bqlFieldNode = ClassDeclaration(
									attributeLists: default,
									modifiers,
									Identifier(bqlFieldName), typeParameterList: null, baseTypesListNode,
									constraintClauses: default, members: default)
								.WithOpenBraceToken(
									Token(leading: TriviaList(), SyntaxKind.OpenBraceToken, TriviaList()));

			var clostBracketToken = isFirstField
				? Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken,
						TriviaList(CarriageReturn, LineFeed, CarriageReturn, LineFeed))
				: Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken, TriviaList(CarriageReturn, LineFeed));

			bqlFieldNode = bqlFieldNode.WithCloseBraceToken(clostBracketToken);

			if (adjacentMemberToCopyRegions != null)
				bqlFieldNode = CopyRegionsFromMember(bqlFieldNode, adjacentMemberToCopyRegions);

			return bqlFieldNode;
		}

		private static ClassDeclarationSyntax CopyRegionsFromMember(ClassDeclarationSyntax bqlFieldNode, 
																	MemberDeclarationSyntax adjacentMemberToCopyRegions)
		{
			var leadingTrivia = adjacentMemberToCopyRegions.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return bqlFieldNode;

			var bqlFieldNodeWithCopiedRegions = CodeGeneration.CopyRegionsFromTrivia(bqlFieldNode, leadingTrivia);
			return bqlFieldNodeWithCopiedRegions;
		}
	}
}
