using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	internal static class BaseMethodReferenceInXmlDocComment
	{
		public static bool HasCorrectReference(SemanticModel semanticModel, MethodDeclarationSyntax patchMethodNode, 
												IMethodSymbol baseMethod, CancellationToken cancellation)
		{
			SyntaxTriviaList leadingTrivia = patchMethodNode.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return false;

			for (int i = 0; i < leadingTrivia.Count; i++)
			{
				var trivia = leadingTrivia[i];

				if (!trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
					trivia.GetStructure() is not DocumentationCommentTriviaSyntax xmlDocCommentNode ||
					xmlDocCommentNode.Content.Count == 0)
				{
					continue;
				}

				if (HasCorrectXmlDocCommentWithReferenceToBaseMethod(semanticModel, xmlDocCommentNode, baseMethod, cancellation))
					return true;
			}

			return false;
		}

		private static bool HasCorrectXmlDocCommentWithReferenceToBaseMethod(SemanticModel semanticModel, DocumentationCommentTriviaSyntax xmlDocCommentNode, 
																			 IMethodSymbol baseMethod, CancellationToken cancellation)
		{
			for (int i = 0; i < xmlDocCommentNode.Content.Count; i++)
			{
				var xmlNode = xmlDocCommentNode.Content[i];

				if (!IsSeeAlsoWithCrefAttributeToBaseMethod(semanticModel, xmlNode, baseMethod, cancellation))
					continue;

				// The <seealso cref="BaseMethod"/> element is found. Now we need to check the "Overrides" prefix
				var previousText = i > 0
					? xmlDocCommentNode.Content[i - 1] as XmlTextSyntax
					: null;

				if (previousText == null)
					continue;

				if (HasOverridesPrefix(previousText))
					return true;
			}

			return false;
		} 

		private static bool IsSeeAlsoWithCrefAttributeToBaseMethod(SemanticModel semanticModel, XmlNodeSyntax xmlNode,
																   IMethodSymbol baseMethod, CancellationToken cancellation)
		{
			const string seeAlsoName = "seealso";
			string? elementName = xmlNode.GetDocTagName().NullIfWhiteSpace();

			if (seeAlsoName != elementName)
				return false;

			SyntaxList<XmlAttributeSyntax>? attributes = xmlNode switch
			{
				XmlEmptyElementSyntax xmlEmptyElement => xmlEmptyElement.Attributes,
				XmlElementSyntax xmlElement 		  => xmlElement.StartTag?.Attributes,
				_ 									  => null
			};

			if (attributes?.Count is null or 0)
				return false;

			for (int i = 0; i < attributes.Value.Count; i++)
			{
				if (attributes.Value[i] is not XmlCrefAttributeSyntax crefAttribute)
					continue;

				var referencedSymbol = semanticModel.GetSymbolOrBestCandidate(crefAttribute.Cref, cancellation);

				if (DoesReferencedSymbolPointToBaseMethod(referencedSymbol, baseMethod))
					return true;
			}

			return false;
		}

		private static bool DoesReferencedSymbolPointToBaseMethod(ISymbol? referencedSymbol, IMethodSymbol baseMethod)
		{
			var referencedMethod = (referencedSymbol, baseMethod.MethodKind) switch
			{
				(IMethodSymbol methodSymbol, _)							 => methodSymbol,
				(IPropertySymbol propertySymbol, MethodKind.PropertyGet) => propertySymbol.GetMethod,
				(IPropertySymbol propertySymbol, MethodKind.PropertySet) => propertySymbol.SetMethod,
				(IEventSymbol eventSymbol, MethodKind.EventAdd)			 => eventSymbol.AddMethod,
				(IEventSymbol eventSymbol, MethodKind.EventRemove)		 => eventSymbol.RemoveMethod,
				(IEventSymbol eventSymbol, MethodKind.EventRaise)		 => eventSymbol.RaiseMethod,
				_														 => null
			};

			return referencedMethod != null &&
				  (referencedMethod.Equals(baseMethod, SymbolEqualityComparer.Default) ||
				   referencedMethod.OriginalDefinition.Equals(baseMethod.OriginalDefinition, SymbolEqualityComparer.Default));
		}

		private static bool HasOverridesPrefix(XmlTextSyntax previousText)
		{
			const string overridesPrefix = "Overrides";

			if (previousText.TextTokens.Count == 0)
				return false;

			for (int i = 0; i < previousText.TextTokens.Count; i++)
			{
				var token = previousText.TextTokens[i];
				
				if (token.IsKind(SyntaxKind.XmlTextLiteralToken))
				{
					string text = token.Text.Trim();

					if (text.Equals(overridesPrefix, StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}

			return false;
		}
	}
}
