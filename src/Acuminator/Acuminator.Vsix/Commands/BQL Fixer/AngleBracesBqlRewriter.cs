using Acuminator.Vsix.Formatter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Acuminator.Vsix.BqlFixer
{
	internal class AngleBracesBqlRewriter : CSharpSyntaxRewriter
	{
		public AngleBracesBqlRewriter(SemanticModel semanticModel)
		{
			SemanticModel = semanticModel;
		}

		protected SemanticModel SemanticModel { get; }

		//public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
		//{
		//	VariableDeclaratorSyntax declarator = node.Declaration.Variables.First();
		//	TypeSyntax variableTypeName = node.Declaration.Type;

		//	return base.VisitFieldDeclaration(node);
		//}

		public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
		{
			if (!(node.Type is GenericNameSyntax baseNode)
				|| !baseNode.ContainsSkippedText
				|| IsClosedNode(baseNode))
				return node;

			var baseArguments = baseNode.TypeArgumentList.Arguments;
			if (baseArguments.Count == 0)
				return node;

			if (!(baseArguments.Last() is GenericNameSyntax lastNode))
				return node;

			// reconstruct all nodes except last
			var typeSyntaxes = DeconstructLastNode(baseNode, out var identifierName);


			
			//var variableDeclaration = SyntaxFactory
			//	.VariableDeclaration(SyntaxFactory.ParseTypeName("bool"))
			//	.AddVariables(SyntaxFactory.VariableDeclarator("canceled"));

			//// Create a field declaration: (private bool canceled;)
			//var fieldDeclaration = SyntaxFactory
			//	.FieldDeclaration(variableDeclaration)
			//	.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));



			return null;
		}

		private bool IsClosedNode(GenericNameSyntax node)
		{
			// if ends with angle braces - it is closed node
			return node.TypeArgumentList.GreaterThanToken.ValueText == ">";
		}

		private bool HasFieldNameNode(GenericNameSyntax node)
		{
			// check for semicolon
			if (node.TypeArgumentList.GreaterThanToken.HasTrailingTrivia
				// actually this is semicolon
				// todo: check if this works OK
				&& node.TypeArgumentList.GreaterThanToken.TrailingTrivia.LastIndexOf(SyntaxKind.SkippedTokensTrivia) >= 0)
			{
				return true;
			}
			return false;
		}

		private IList<(IList<TypeSyntax> nodes, GenericNameSyntax lastNode)> DeconstructLastNode(
			GenericNameSyntax node,
			out IdentifierNameSyntax identifierName)
		{
			IdentifierNameSyntax name = null;
			var result = DeconstructLastNodeRecursively(node, true, false, name_ => name = name_).ToArray();
			identifierName = name;
			return result;
		}

		private IEnumerable<(IList<TypeSyntax>, GenericNameSyntax)> DeconstructLastNodeRecursively(
			GenericNameSyntax node,
			bool checkHasSemicolon,
			bool checkIdentifier,
			Action<IdentifierNameSyntax> fieldNameUtilizer)
		{
			var args = node.TypeArgumentList.Arguments;
			var lastNode = args.Last() as GenericNameSyntax;
			var checkIdentifier_ = checkIdentifier;
			if (lastNode == null || IsClosedNode(lastNode))
			{
				IEnumerable<TypeSyntax> newArgs = args;
				if(checkIdentifier && lastNode == null && args.Last() is IdentifierNameSyntax identifier)
				{
					fieldNameUtilizer(identifier);
					checkIdentifier_ = false;
					newArgs = args.Take(args.Count - 1);
				}
				
				yield return (newArgs.ToList(), null);
				yield break;
			}
			
			bool checkHasSemicolon_ = checkHasSemicolon;
			if (checkHasSemicolon && HasFieldNameNode(node))
			{
				yield return (args.Take(args.Count -1).ToList(), lastNode);
				checkHasSemicolon_ = false;
				checkIdentifier_ = true;
			}

			foreach (var inner in DeconstructLastNodeRecursively(lastNode, checkHasSemicolon_, checkIdentifier_, fieldNameUtilizer))
			{
				yield return inner;
			}
		}


		private TypeArgumentListSyntax GetTypeArgumentList(TypeSyntax typeSyntax)
		{
			if (!(typeSyntax is GenericNameSyntax generic))
				return null;

			return generic.TypeArgumentList;
		}

		private GenericNameSyntax GetLastGenericName(GenericNameSyntax typeSyntax)
		{
			var args = GetTypeArgumentList(typeSyntax);
			if (args == null) return typeSyntax;

			var last = args.Arguments.LastOrDefault() as GenericNameSyntax;
			if (last == null)
				return typeSyntax;

			return GetLastGenericName(last);
		}
	}
}
