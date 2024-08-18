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

			var excludeTagNode = XmlEmptyElement(XmlCommentsConstants.ExcludeTag);
			var xmlExcludeTrivia = Trivia(
				DocumentationComment(
					XmlText(string.Empty),
					excludeTagNode)
			);

			cancellation.ThrowIfCancellationRequested();

			var newMemberDeclaration = AddDocumentationTrivia(memberDeclaration, index: 0, xmlExcludeTrivia);
			var newRootNode 		 = rootNode.ReplaceNode(memberDeclaration, newMemberDeclaration);
			var newDocument 		 = Document.WithSyntaxRoot(newRootNode);

			return newDocument;
		}
	}
}