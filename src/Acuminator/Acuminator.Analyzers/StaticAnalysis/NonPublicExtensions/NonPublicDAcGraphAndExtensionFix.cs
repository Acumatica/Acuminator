#nullable enable

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

namespace Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class NonPublicDAcGraphAndExtensionFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
			new HashSet<string>
			{
				Descriptors.PX1022_NonPublicDac.Id,
				Descriptors.PX1022_NonPublicDacExtension.Id,
				Descriptors.PX1022_NonPublicGraph.Id,
				Descriptors.PX1022_NonPublicGraphExtension.Id
			}
			.ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var diagnostics = context.Diagnostics;

			if (diagnostics.IsDefaultOrEmpty)
				return Task.CompletedTask;

			var supportedDiagnostics = FixableDiagnosticIds;

			if (supportedDiagnostics.IsDefaultOrEmpty)
				return Task.CompletedTask;

			if (diagnostics.Length == 1)
			{
				var diagnostic = diagnostics[0];

				if (!supportedDiagnostics.Contains(diagnostic.Id) || GetCheckedSymbolKind(diagnostic) is not CheckedSymbolKind checkedSymbolKind)
					return Task.CompletedTask;

				return RegisterCodeFixAsync(diagnostic, context, checkedSymbolKind);
			}
			else
			{
				var allTasks = new List<Task>(capacity: diagnostics.Length);

				foreach (Diagnostic diagnostic in diagnostics)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					if (!supportedDiagnostics.Contains(diagnostic.Id) || GetCheckedSymbolKind(diagnostic) is not CheckedSymbolKind checkedSymbolKind)
						continue;

					Task diagnosticTask = RegisterCodeFixAsync(diagnostic, context, checkedSymbolKind);
					allTasks.Add(diagnosticTask);
				}

				return Task.WhenAll(allTasks);
			}
		}

		private static CheckedSymbolKind? GetCheckedSymbolKind(Diagnostic diagnostic)
		{
			if (diagnostic.Properties.TryGetValue(nameof(CheckedSymbolKind), out string checkedSymbolKindStr) &&
				Enum.TryParse(checkedSymbolKindStr, out CheckedSymbolKind checkedSymbolKind))
			{
				return checkedSymbolKind;
			}

			return null;
		}

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