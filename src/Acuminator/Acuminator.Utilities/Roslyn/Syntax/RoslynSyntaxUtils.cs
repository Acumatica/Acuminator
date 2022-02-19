#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public static class RoslynSyntaxUtils
	{
		public static bool IsLocalVariable(this SemanticModel semanticModel, MethodDeclarationSyntax containingMethod, string? variableName)
		{
			semanticModel.ThrowOnNull(nameof(semanticModel));
			containingMethod.ThrowOnNull(nameof(containingMethod));

			if (variableName.IsNullOrWhiteSpace())
				return false;

			if (containingMethod.Body == null)
				return false;

			DataFlowAnalysis dataFlowAnalysis = semanticModel.AnalyzeDataFlow(containingMethod.Body);

			if (dataFlowAnalysis == null || !dataFlowAnalysis.Succeeded)
				return false;

			return dataFlowAnalysis.VariablesDeclared.Any(var => var.Name == variableName);
		}

		/// <summary>
		/// Try to get the size of the single dimensional non jagged array from array creation expression. If failed then return <c>null</c>. 
		///  Only simple and sensible cases of static expressions are handled. 
		/// The cast expressions, results of method calls or variables changed at runtime and passed as array size are not handled.
		/// </summary>
		/// <param name="arrayCreationExpression">The array creation expression.</param>
		/// <param name="semanticModel">(Optional) The semantic model. Null by default. Can be passed to try to fallback to its constant evaluation if
		/// 							simple case don't work.</param>
		/// <param name="cancellationToken">(Optional) The cancellation token.</param>
		/// <returns/>
		public static int? TryGetSizeOfSingleDimensionalNonJaggedArray(ExpressionSyntax? arrayCreationExpression, SemanticModel? semanticModel = null,
																	   CancellationToken cancellationToken = default)
		{
			return arrayCreationExpression switch
			{
				ArrayCreationExpressionSyntax arrayCreation
					when arrayCreation.Initializer != null => arrayCreation.Initializer.Expressions.Count,

				ArrayCreationExpressionSyntax arrayCreationWithouInitializer => TryGetSizeOfSingleDimensionalNonJaggedArray(arrayCreationWithouInitializer.Type, 
																															semanticModel, cancellationToken),
				ImplicitArrayCreationExpressionSyntax implicitArrayCreation  => implicitArrayCreation.Initializer?.Expressions.Count,
				InitializerExpressionSyntax initializerExpression
					when initializerExpression.IsKind(SyntaxKind.ArrayInitializerExpression) => initializerExpression.Expressions.Count,

				StackAllocArrayCreationExpressionSyntax stackAllocArrayCreation
					when stackAllocArrayCreation.Type is ArrayTypeSyntax arrayType => TryGetSizeOfSingleDimensionalNonJaggedArray(arrayType, semanticModel,
																																  cancellationToken),
				_ => null
			};
		}

		/// <summary>
		/// Try to get the size of single dimensional non jagged array. If failed then return <c>null</c>.
		/// Only simple and sensible cases of static expressions are handled.
		/// The cast expressions, results of method calls or variables changed at runtime and passed as array size are not handled
		/// </summary>
		/// <param name="arrayType">The arrayType node to act on.</param>
		/// <param name="semanticModel">(Optional) The semantic model. Null by default. 
		/// 							Can be passed to try to fallback to its constant evaluation if simple case don't work.</param>
		/// <param name="cancellationToken">(Optional) The cancellation token.</param>
		/// <returns/>
		public static int? TryGetSizeOfSingleDimensionalNonJaggedArray(ArrayTypeSyntax? arrayType, SemanticModel? semanticModel = null,
																	   CancellationToken cancellationToken = default)
		{
			if (arrayType == null)
				return null;

			var rankSpecifiers = arrayType.RankSpecifiers;

			if (rankSpecifiers.Count != 1)  //not a jagged array
				return null;

			var rankNode = rankSpecifiers[0];

			if (rankNode.Rank != 1)   //Single-dimensional array
				return null;

			//Only simple and sensible cases of static expressions are handled here
			//The cast expressions, results of method calls or variables changed at runtime and passed as array size are not handled
			if (rankNode.Sizes[0] is LiteralExpressionSyntax literalExpression)
				return literalExpression.Token.Value as int?;

			if (semanticModel == null)
				return null;

			var constantValue = semanticModel.GetConstantValue(rankNode.Sizes[0], cancellationToken);
			return constantValue.HasValue
				? constantValue.Value as int?
				: null;
		}

		/// <summary>
		/// An ISymbol extension method that gets syntax for symbol asynchronously.
		/// </summary>
		/// <param name="symbol">The symbol to act on.</param>
		/// <param name="cancellationToken">(Optional) The cancellation token.</param>
		/// <returns>
		/// An asynchronous task that yields the syntax.
		/// </returns>
		public static Task<SyntaxNode?> GetSyntaxAsync(this ISymbol? symbol, CancellationToken cancellationToken = default)
		{
			if (symbol == null)
				return Task.FromResult<SyntaxNode?>(null);

			var declarations = symbol.DeclaringSyntaxReferences;

			if (declarations.Length == 0)
				return Task.FromResult<SyntaxNode?>(null);

			return declarations[0].GetSyntaxAsync(cancellationToken);
		}

		public static SyntaxNode? GetSyntax(this ISymbol? symbol, CancellationToken cancellationToken = default)
		{
			if (symbol == null)
				return null;

			var declarations = symbol.DeclaringSyntaxReferences;

			if (declarations.Length == 0)
				return null;

			return declarations[0].GetSyntax(cancellationToken);
		}

		public static Location? GetLocation(this AttributeData? attribute, CancellationToken cancellationToken = default) =>
			attribute?.ApplicationSyntaxReference
					 ?.GetSyntax(cancellationToken)
					 ?.GetLocation();

		public static IEnumerable<SyntaxToken> GetIdentifiers(this MemberDeclarationSyntax? member) =>
			member switch
			{
				PropertyDeclarationSyntax propertyDeclaration       => propertyDeclaration.Identifier.ToEnumerable(),
				FieldDeclarationSyntax fieldDeclaration             => fieldDeclaration.Declaration.Variables.Select(variable => variable.Identifier),
				MethodDeclarationSyntax methodDeclaration           => methodDeclaration.Identifier.ToEnumerable(),
				EventDeclarationSyntax eventDeclaration             => eventDeclaration.Identifier.ToEnumerable(),                                           //for explicit event declaration with "add" and "remove"
				EventFieldDeclarationSyntax eventFieldDeclaration   => eventFieldDeclaration.Declaration.Variables.Select(variable => variable.Identifier),  //for field event declaration
				DelegateDeclarationSyntax delegateDeclaration       => delegateDeclaration.Identifier.ToEnumerable(),
				ClassDeclarationSyntax nestedClassDeclaration       => nestedClassDeclaration.Identifier.ToEnumerable(),
				EnumDeclarationSyntax enumDeclaration               => enumDeclaration.Identifier.ToEnumerable(),
				StructDeclarationSyntax structDeclaration           => structDeclaration.Identifier.ToEnumerable(),
				InterfaceDeclarationSyntax interfaceDeclaration     => interfaceDeclaration.Identifier.ToEnumerable(),
				ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.ToEnumerable(),
				_                                                   => Enumerable.Empty<SyntaxToken>()
			};

		public static Accessibility? GetAccessibility(this MemberDeclarationSyntax member, SemanticModel semanticModel,
													 CancellationToken cancellationToken = default)
		{
			member.ThrowOnNull(nameof(member));
			semanticModel.ThrowOnNull(nameof(semanticModel));
			
			switch (member)
			{		
				case FieldDeclarationSyntax fieldDeclaration:
					VariableDeclaratorSyntax firstFieldDeclaration = fieldDeclaration.Declaration.Variables.FirstOrDefault();
					return firstFieldDeclaration != null 
						? semanticModel.GetDeclaredSymbol(firstFieldDeclaration, cancellationToken)?.DeclaredAccessibility
						: null;
				case EventFieldDeclarationSyntax eventFieldDeclaration:
					VariableDeclaratorSyntax firstEventDeclaration = eventFieldDeclaration.Declaration.Variables.FirstOrDefault();     //for field event declaration
					return firstEventDeclaration != null 
						? semanticModel.GetDeclaredSymbol(firstEventDeclaration, cancellationToken)?.DeclaredAccessibility
						: null;
				default:
					return semanticModel.GetDeclaredSymbol(member, cancellationToken)?.DeclaredAccessibility;
			}
		}

		public static bool IsPublic(this MemberDeclarationSyntax? member)
		{
			SyntaxTokenList modifiers = member.GetModifiers();
			return modifiers.Any(SyntaxKind.PublicKeyword);
		}

		public static bool IsPartial(this TypeDeclarationSyntax typeDeclaration) =>
			typeDeclaration.CheckIfNull(nameof(typeDeclaration))
						   .Modifiers
						   .Any(SyntaxKind.PartialKeyword);
		
		public static bool IsInternal(this MemberDeclarationSyntax member)
		{
			SyntaxTokenList modifiers = member.GetModifiers();

			//Exclude private internal modifiers from C# 7 whish are actually for members visible only from internal inherited classes 
			return modifiers.Any(SyntaxKind.InternalKeyword) && !modifiers.Any(SyntaxKind.PrivateKeyword);  
		}

		public static SyntaxTokenList GetModifiers(this MemberDeclarationSyntax? member) =>
			member.CheckIfNull(nameof(member)) switch
			{
				BasePropertyDeclarationSyntax basePropertyDeclaration => basePropertyDeclaration.Modifiers,
				BaseMethodDeclarationSyntax baseMethodDeclaration     => baseMethodDeclaration.Modifiers,
				BaseTypeDeclarationSyntax baseTypeDeclaration         => baseTypeDeclaration.Modifiers,
				BaseFieldDeclarationSyntax baseFieldDeclaration       => baseFieldDeclaration.Modifiers,
				DelegateDeclarationSyntax delegateDeclaration         => delegateDeclaration.Modifiers,
				_                                                     => SyntaxFactory.TokenList()
			};

		/// <summary>
		/// Returns the body of the <paramref name="node"/> that is passed as an argument, if it is available
		/// (e.g., method body for getters, methods, constructors, expression for expression-bodied getters and methods, etc.)
		/// </summary>
		/// <param name="node">Syntax node</param>
		/// <returns>Syntax node for the body, if any</returns>
		public static CSharpSyntaxNode? GetBody(this SyntaxNode? node) =>
			node switch
			{
				AccessorDeclarationSyntax accessorSyntax => accessorSyntax.Body,
				MethodDeclarationSyntax methodSyntax => methodSyntax.Body ?? (methodSyntax.ExpressionBody?.Expression as CSharpSyntaxNode),
				ConstructorDeclarationSyntax constructorSyntax => constructorSyntax.Body,
				_ => null,
			};

		public static IEnumerable<AttributeSyntax> GetAttributes(this MemberDeclarationSyntax member)
		{
			member.ThrowOnNull(nameof(member));
			return GetAttributesImpl();

			IEnumerable<AttributeSyntax> GetAttributesImpl()
			{
				var attributeLists = member.GetAttributeLists();

				for (int i = 0; i < attributeLists.Count; i++)
				{
					var attributeList = attributeLists[i].Attributes;

					for (int j = 0; j < attributeList.Count; j++)
					{
						yield return attributeList[j];
					}
				}
			}
		}

		public static SyntaxList<AttributeListSyntax> GetAttributeLists(this MemberDeclarationSyntax? member) =>
			member switch
			{
				PropertyDeclarationSyntax propertyDeclaration       => propertyDeclaration.AttributeLists,
				FieldDeclarationSyntax fieldDeclaration             => fieldDeclaration.AttributeLists,
				MethodDeclarationSyntax methodDeclaration           => methodDeclaration.AttributeLists,
				EventDeclarationSyntax eventDeclaration             => eventDeclaration.AttributeLists,
				EventFieldDeclarationSyntax eventFieldDeclaration   => eventFieldDeclaration.AttributeLists,
				DelegateDeclarationSyntax delegateDeclaration       => delegateDeclaration.AttributeLists,
				ClassDeclarationSyntax nestedClassDeclaration       => nestedClassDeclaration.AttributeLists,
				EnumDeclarationSyntax enumDeclaration               => enumDeclaration.AttributeLists,
				StructDeclarationSyntax structDeclaration           => structDeclaration.AttributeLists,
				InterfaceDeclarationSyntax interfaceDeclaration     => interfaceDeclaration.AttributeLists,
				ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.AttributeLists,
				_                                                   => new SyntaxList<AttributeListSyntax>()
			};

		public static SyntaxTrivia ToSingleLineComment(this string? commentContent)
		{
			const string commentPrefix = "//";
			string comment = commentContent.IsNullOrWhiteSpace()
				? commentPrefix
				: commentPrefix + " " + commentContent!.Trim();

			return SyntaxFactory.Comment(comment);
		}
	}
}
