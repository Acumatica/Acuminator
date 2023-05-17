#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix
{
	internal class AddExcludeTagCodeAction : AddDocumentationTagCodeAction
	{
		public AddExcludeTagCodeAction(Document document, TextSpan span) : 
								  base(nameof(Resources.PX1007FixAddExcludeTag).GetLocalized().ToString(), document, span)
		{
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await Document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (rootNode?.FindNode(Span) is not MemberDeclarationSyntax memberDeclaration)
				return Document;

			var xmlExcludeTrivia = Trivia(
				DocumentationComment(
					XmlEmptyElement(XmlCommentsConstants.ExcludeTag),
					XmlText(XmlTextNewLine)
				)
			);

			cancellation.ThrowIfCancellationRequested();

			var newRootNode = AddDocumentationTrivia(rootNode, memberDeclaration, xmlExcludeTrivia, index: 0);
			var newDocument = Document.WithSyntaxRoot(newRootNode);

			return newDocument;
		}
	}
}