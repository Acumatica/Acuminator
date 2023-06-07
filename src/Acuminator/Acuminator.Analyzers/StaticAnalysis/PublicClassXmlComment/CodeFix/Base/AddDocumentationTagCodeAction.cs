#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix
{
	internal abstract class AddDocumentationTagCodeAction : CodeAction
	{
		private readonly string _title;

		public sealed override string Title => _title;

		public sealed override string EquivalenceKey => _title;

		protected Document Document { get; }

		protected TextSpan Span { get; }

		protected Workspace Workspace => Document.Project.Solution.Workspace;

		public AddDocumentationTagCodeAction(string title, Document document, TextSpan span)
		{
			_title 	 = title;
			Document = document;
			Span 	 = span;
		}

		protected MemberDeclarationSyntax AddDocumentationTrivia(MemberDeclarationSyntax memberDeclaration, int index, 
																 in SyntaxTrivia documentationTrivia)
		{
			SyntaxTrivia lineFeed 		 			 = SyntaxFactory.LineFeed;
			SyntaxTriviaList leadingTrivia 	 		 = memberDeclaration.GetLeadingTrivia();
			bool appendLineFeedToEndOfInsertedTrivia = !leadingTrivia.ContainsNewLine();
			bool prependLineFeedBeforeInsertedTrivia = !IsFirstMemberOfTypeOrNamespace(memberDeclaration);

			SyntaxTriviaList newTrivia = appendLineFeedToEndOfInsertedTrivia
				? leadingTrivia.Insert(index, lineFeed)
				: leadingTrivia;

			newTrivia = newTrivia.Insert(index, documentationTrivia);

			if (prependLineFeedBeforeInsertedTrivia)
				newTrivia = newTrivia.Insert(index, lineFeed);

			var newMemberDeclaration = memberDeclaration.WithLeadingTrivia(newTrivia);
			return newMemberDeclaration;
		}

		private bool IsFirstMemberOfTypeOrNamespace(MemberDeclarationSyntax memberDeclaration) =>
			memberDeclaration.Parent switch
			{
				TypeDeclarationSyntax containingType 			=> memberDeclaration.Equals(containingType.Members[0]),
				NamespaceDeclarationSyntax namespaceDeclaration => memberDeclaration.Equals(namespaceDeclaration.Members[0]),
				_ 												=> false,
			};
	}
}