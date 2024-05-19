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
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class NonPublicDacGraphAndExtensionFix : CodeFixProvider
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

		private Task RegisterCodeFixAsync(Diagnostic diagnostic, CodeFixContext context, CheckedSymbolKind checkedSymbolKind)
		{
			if (GetCodeActionName(checkedSymbolKind) is not string codeActionName)
				return Task.CompletedTask;

			var codeAction = CodeAction.Create(codeActionName,
											   cToken => MakeTypePublicAsync(context.Document, context.Span, diagnostic.AdditionalLocations, cToken),
											   equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private static string? GetCodeActionName(CheckedSymbolKind checkedSymbolKind) =>
			checkedSymbolKind switch
			{
				CheckedSymbolKind.Dac 			 => nameof(Resources.PX1022DacFix).GetLocalized().ToString(),
				CheckedSymbolKind.Graph 		 => nameof(Resources.PX1022GraphFix).GetLocalized().ToString(),
				CheckedSymbolKind.DacExtension 	 => nameof(Resources.PX1022DacExtensionFix).GetLocalized().ToString(),
				CheckedSymbolKind.GraphExtension => nameof(Resources.PX1022GraphExtensionFix).GetLocalized().ToString(),
				_ 								 => null
			};

		private async Task<Solution> MakeTypePublicAsync(Document document, TextSpan span, IReadOnlyList<Location> otherPartialTypeDeclarations, 
														 CancellationToken cancellationToken)
		{
			Solution originalSolution = document.Project.Solution;
			var (semanticModel, root) = await document.GetSemanticModelAndRootAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null || root == null)
				return originalSolution;

			SyntaxNode? diagnosticNode = root.FindNode(span);
			var graphOrDacOrExtensionToMakePublicNode = (diagnosticNode as ClassDeclarationSyntax) ?? diagnosticNode?.Parent<ClassDeclarationSyntax>();

			if (graphOrDacOrExtensionToMakePublicNode == null)
				return originalSolution;

			cancellationToken.ThrowIfCancellationRequested();

			var graphOrDacOrExtType = semanticModel.GetDeclaredSymbol(graphOrDacOrExtensionToMakePublicNode, cancellationToken);
			
			if (graphOrDacOrExtType == null) 
				return originalSolution;

			var changedSolution = MakeTypeNodePublic(document, root, graphOrDacOrExtensionToMakePublicNode);
			var typeDeclarations = graphOrDacOrExtType.DeclaringSyntaxReferences;

			if (typeDeclarations.Length <= 1)
				return changedSolution;

			foreach (var typeDeclaration in typeDeclarations)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (typeDeclaration.SyntaxTree.FilePath == graphOrDacOrExtensionToMakePublicNode.SyntaxTree.FilePath &&
					typeDeclaration.Span == graphOrDacOrExtensionToMakePublicNode.Span)
				{
					continue;
				}

				changedSolution = 
					await MakeTypePartialDeclarationPublicAsync(typeDeclaration, changedSolution, originalSolution, cancellationToken)
							.ConfigureAwait(false);
			}

			return changedSolution;
		}

		private async Task<Solution> MakeTypePartialDeclarationPublicAsync(SyntaxReference typeDeclaration, Solution changedSolution, 
																		   Solution originalSolution, CancellationToken cancellation)
		{
			DocumentId? docIdContainingDeclaration = originalSolution.GetDocumentId(typeDeclaration.SyntaxTree);

			if (docIdContainingDeclaration == null)
				return changedSolution;

			// Obtain document through the doc ID to avoid possibility that document from the changed solution
			// can't be found with a syntax tree from the original solution
			var docContainingDeclaration = changedSolution.GetDocument(docIdContainingDeclaration);

			if (docContainingDeclaration == null)
				return changedSolution;

			var docRoot = await docContainingDeclaration.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var typeDeclarationNode = docRoot?.FindNode(typeDeclaration.Span) as ClassDeclarationSyntax;

			if (typeDeclarationNode == null)
				return changedSolution;

			changedSolution = MakeTypeNodePublic(docContainingDeclaration, docRoot!, typeDeclarationNode);
			return changedSolution;
		}

		private Solution MakeTypeNodePublic(Document document, SyntaxNode root, ClassDeclarationSyntax typeNodeToMakePublicNode)
		{
			List<TypeDeclarationSyntax> nodesToMakePublic = GetTypeNodesToMakePublic(typeNodeToMakePublicNode).ToList();
			SyntaxNode trackingRoot = root!.TrackNodes(nodesToMakePublic);

			foreach (TypeDeclarationSyntax nonPublicTypeNode in nodesToMakePublic)
			{
				var nonPublicTypeNodeFromModifiedTree = trackingRoot.GetCurrentNode(nonPublicTypeNode);

				if (nonPublicTypeNodeFromModifiedTree != null)
				{
					var publicTypeNode = MakeTypeNodePublic(nonPublicTypeNodeFromModifiedTree);
					trackingRoot = trackingRoot.ReplaceNode(nonPublicTypeNodeFromModifiedTree, publicTypeNode);
				}
			}

			var changedDocument = document.WithSyntaxRoot(trackingRoot);
			return changedDocument.Project.Solution;
		}

		private IEnumerable<TypeDeclarationSyntax> GetTypeNodesToMakePublic(TypeDeclarationSyntax typeNodeToMakePublicNode)
		{
			TypeDeclarationSyntax? currentTypeNode = typeNodeToMakePublicNode;

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