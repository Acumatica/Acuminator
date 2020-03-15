using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.NonPublicExtensions
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class NonPublicExtensionFix : CodeFixProvider
	{
		private const string DiagnosticID = "PX1022";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticID);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == DiagnosticID);

			if (diagnostic == null || !diagnostic.Properties.TryGetValue(nameof(ExtensionType), out string extensionType))
				return Task.CompletedTask;
			
			string codeFixResourceToUse = extensionType == ExtensionType.DAC
				? nameof(Resources.PX1022DacFix)
				: nameof(Resources.PX1022GraphFix);

			string codeActionName = codeFixResourceToUse.GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName,
													  cToken => MakeExtensionPublicAsync(context.Document, context.Span, cToken),
													  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
			return Task.CompletedTask;
		}

		private async Task<Document> MakeExtensionPublicAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode diagnosticNode = root?.FindNode(span);
			var extensionNode = (diagnosticNode as ClassDeclarationSyntax) ?? diagnosticNode?.Parent<ClassDeclarationSyntax>();

			if (extensionNode == null || cancellationToken.IsCancellationRequested)
				return document;

			List<TypeDeclarationSyntax> nodesToMakePublic = GetTypeNodesToMakePublic(extensionNode).ToList();
			SyntaxNode trackingRoot = root.TrackNodes(nodesToMakePublic);

			foreach (TypeDeclarationSyntax nonPublicTypeNode in nodesToMakePublic)
			{
				var nonPublicTypeNodeFromModifiedTree = trackingRoot.GetCurrentNode(nonPublicTypeNode);

				if (nonPublicTypeNodeFromModifiedTree != null)
				{
					var publicTypeNode = MakeTypeNodePublic(nonPublicTypeNodeFromModifiedTree);
					trackingRoot = trackingRoot.ReplaceNode(nonPublicTypeNodeFromModifiedTree, publicTypeNode);
				}
			}

			return document.WithSyntaxRoot(trackingRoot);
		}

		private IEnumerable<TypeDeclarationSyntax> GetTypeNodesToMakePublic(TypeDeclarationSyntax extensionNode)
		{
			TypeDeclarationSyntax currentTypeNode = extensionNode;

			while (currentTypeNode != null)
			{
				if (!currentTypeNode.Modifiers.Any(SyntaxKind.PublicKeyword))
				{
					yield return currentTypeNode;
				}

				currentTypeNode = currentTypeNode.Parent<TypeDeclarationSyntax>();
			}
		}

		private TypeDeclarationSyntax MakeTypeNodePublic(TypeDeclarationSyntax nonPublicTypeNode)
		{
			var modifiersWithPublicAccesibility = 
				SyntaxFactory.TokenList(
					SyntaxFactory.Token(SyntaxKind.PublicKeyword));

			var modifiersToTransfer = nonPublicTypeNode.Modifiers.Where(token => !SyntaxFacts.IsAccessibilityModifier(token.Kind()));
			modifiersWithPublicAccesibility = modifiersWithPublicAccesibility.AddRange(modifiersToTransfer);
			return nonPublicTypeNode.WithModifiers(modifiersWithPublicAccesibility);
		}
	}
}