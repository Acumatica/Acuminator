using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class KeyFieldDeclarationFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1055_DacKeyFieldBound.Id);

		public override  async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1055_DacKeyFieldBound.Id);

			if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
				return;

			SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			SyntaxNode codeFixNode = root?.FindNode(context.Span);
			AttributeSyntax attributeNode = codeFixNode as AttributeSyntax;

			string codeActionName = nameof(Resources.PX1055Fix1).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName,
														  cToken => MakeIdentityFieldKeyAsync(context.Document, context.Span, cToken),
														  equivalenceKey: codeActionName);
			return;
		}

		private async Task<Document> MakeIdentityFieldKeyAsync(Document document, TextSpan span, CancellationToken cToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cToken).ConfigureAwait(false);
			SyntaxNode diagnosticNode = root?.FindNode(span);

			if (diagnosticNode == null || cToken.IsCancellationRequested)
				return document;

			var dacSyntaxNode = diagnosticNode.Parent;


			return document.WithSyntaxRoot(root);
		}

		
	}
}
