#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.CodeActions;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
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