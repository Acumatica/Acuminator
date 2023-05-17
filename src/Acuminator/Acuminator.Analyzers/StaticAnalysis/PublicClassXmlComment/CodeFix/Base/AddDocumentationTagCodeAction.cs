#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix
{
	internal abstract class AddDocumentationTagCodeAction : CodeAction
	{
		protected const string XmlTextNewLine = "\n";

		private readonly string _title;

		public sealed override string Title => _title;

		public sealed override string EquivalenceKey => _title;

		protected Document Document { get; }

		protected TextSpan Span { get; }

		public AddDocumentationTagCodeAction(string title, Document document, TextSpan span)
		{
			_title = title;
			Document = document;
			Span = span;
		}

		protected SyntaxNode AddDocumentationTrivia(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
												    SyntaxTrivia documentationTrivia, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var newTrivia = memberDeclaration.GetLeadingTrivia().Add(documentationTrivia);
			var newClassDeclarationSyntax = memberDeclaration.WithLeadingTrivia(newTrivia);

			return rootNode.ReplaceNode(memberDeclaration, newClassDeclarationSyntax);
		}
	}
}