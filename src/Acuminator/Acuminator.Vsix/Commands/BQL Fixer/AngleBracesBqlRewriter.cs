using Acuminator.Vsix.Formatter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Acuminator.Vsix.BqlFixer
{
	public class AngleBracesBqlRewriter : CSharpSyntaxRewriter
	{
		public AngleBracesBqlRewriter(SemanticModel semanticModel)
		{
			SemanticModel = semanticModel;
		}

		protected SemanticModel SemanticModel { get; }

		public override SyntaxNode VisitIncompleteMember(IncompleteMemberSyntax node)
		{
			if (!(node.Type is GenericNameSyntax baseNode)
				|| IsClosedNode(baseNode))
				return node;

			var baseArguments = baseNode.TypeArgumentList.Arguments;
			if (baseArguments.Count == 0)
				return node;

			// reconstruct all nodes except last
			var typeSyntaxes = DeconstructLastNode(baseNode, out var identifierName);

			for (int i = typeSyntaxes.Count - 2; i >= 0; i--)
			{
				var generic = typeSyntaxes[i].lastNode;
				var args = typeSyntaxes[i + 1].nodes;
				var syntaxList = SyntaxFactory.SeparatedList(args);
				var argSyntax = SyntaxFactory.TypeArgumentList(syntaxList);
				var resGeneric = SyntaxFactory.GenericName(generic.Identifier, argSyntax);
				typeSyntaxes[i].nodes.Add(resGeneric);
			}

			var resulterGeneric = typeSyntaxes[0].nodes.First() as GenericNameSyntax;
			var variableName = SyntaxFactory.VariableDeclarator(identifierName.Identifier);
			var variableDeclaration = SyntaxFactory.VariableDeclaration(resulterGeneric).AddVariables(variableName);
			var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration).WithModifiers(node.Modifiers);

			return fieldDeclaration;
		}

		private bool IsClosedNode(GenericNameSyntax node)
		{
			// if ends with angle braces - it is closed node
			return node.TypeArgumentList.GreaterThanToken.ValueText == ">";
		}

		private IList<(List<TypeSyntax> nodes, GenericNameSyntax lastNode)> DeconstructLastNode(
			GenericNameSyntax node,
			out IdentifierNameSyntax identifierName)
		{
			IdentifierNameSyntax name = null;
			var result = Enumerable
				.Repeat((new List<TypeSyntax>(), node), 1)
				.Union(DeconstructLastNodeRecursively(node, name_ => name = name_))
				.ToArray();
			identifierName = name;
			return result;
		}

		private IEnumerable<(List<TypeSyntax>, GenericNameSyntax)> DeconstructLastNodeRecursively(
			GenericNameSyntax node,
			Action<IdentifierNameSyntax> fieldNameUtilizer)
		{
			var args = node.TypeArgumentList.Arguments;
			var lastNode = args.Last() as GenericNameSyntax;
			if (lastNode == null || IsClosedNode(lastNode))
			{
				IEnumerable<TypeSyntax> newArgs = args;
				if(lastNode == null && args.Last() is IdentifierNameSyntax identifier)
				{
					fieldNameUtilizer(identifier);
					newArgs = args.Take(args.Count - 1).Select(a => a.WithoutTrivia());
				}
				
				yield return (newArgs.ToList(), null);
				yield break;
			}

			yield return (args.Take(args.Count - 1).Select(a => a.WithoutTrivia()).ToList(), lastNode.WithoutTrivia());

			foreach (var inner in DeconstructLastNodeRecursively(lastNode, fieldNameUtilizer))
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
