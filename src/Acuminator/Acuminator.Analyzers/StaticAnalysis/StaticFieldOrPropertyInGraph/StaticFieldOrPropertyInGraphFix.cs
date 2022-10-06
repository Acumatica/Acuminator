#nullable enable

using System;
using System.Collections;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

using static Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature.InvalidPXActionSignatureFix;

namespace Acuminator.Analyzers.StaticAnalysis.StaticFieldOrPropertyInGraph
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class StaticFieldOrPropertyInGraphFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1062_StaticFieldOrPropertyInGraph.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

			if (diagnostic == null)
				return Task.CompletedTask;

			string? codeFixFormatArg = GetCodeFixFormatArg(diagnostic);

			if (codeFixFormatArg.IsNullOrWhiteSpace())
				return Task.CompletedTask;

			bool isViewOrAction = IsViewOrAction(diagnostic);

			if (!isViewOrAction)
			{
				string makeReadOnlyCodeActionFormat = nameof(Resources.PX1062FixMakeReadOnlyFormat).GetLocalized().ToString();
				string makeReadOnlyCodeActionName = string.Format(makeReadOnlyCodeActionFormat, codeFixFormatArg);

				CodeAction makeReadOnlyCodeAction =
					CodeAction.Create(makeReadOnlyCodeActionName,
									  cToken => MakeReadOnly(context.Document, context.Span, cToken),
									  equivalenceKey: makeReadOnlyCodeActionName);
				context.RegisterCodeFix(makeReadOnlyCodeAction, diagnostic);
			}

			string makeNonStaticCodeActionFormat = nameof(Resources.PX1062FixMakeNonStaticFormat).GetLocalized().ToString();
			string makeNonStaticCodeActionName = string.Format(makeNonStaticCodeActionFormat, codeFixFormatArg);

			CodeAction makeNonStaticCodeAction =
					CodeAction.Create(makeNonStaticCodeActionName,
									  cToken => MakeNonStatic(context.Document, context.Span, cToken),
									  equivalenceKey: makeNonStaticCodeActionName);
			context.RegisterCodeFix(makeNonStaticCodeAction, diagnostic);
			return Task.CompletedTask;
		}

		private bool IsViewOrAction(Diagnostic diagnostic) =>
			diagnostic.Properties?.Count > 0 &&
			diagnostic.Properties.TryGetValue(StaticFieldOrPropertyInGraphDiagnosticProperties.IsViewOrAction, out string value) &&
			bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase);

		private string? GetCodeFixFormatArg(Diagnostic diagnostic)
		{
			if (diagnostic.Properties?.Count is null or 0)
				return null;

			return diagnostic.Properties.TryGetValue(StaticFieldOrPropertyInGraphDiagnosticProperties.CodeFixFormatArg, out string value)
				? value
				: null;
		}

		private async Task<Document> MakeReadOnly(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode diagnosticNode = root?.FindNode(span);

			if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;

			ClassDeclarationSyntax dacDeclaration = diagnosticNode.Parent<ClassDeclarationSyntax>();
			string identifierToRemove = diagnosticNode is ClassDeclarationSyntax dacFieldDeclaration
											? dacFieldDeclaration.Identifier.Text
											: (diagnosticNode as PropertyDeclarationSyntax)?.Identifier.Text;

			if (identifierToRemove.IsNullOrWhiteSpace())
				return document;

			var regionsVisitor = new RegionsVisitor(identifierToRemove, cancellationToken);
			regionsVisitor.Visit(dacDeclaration);

			if (cancellationToken.IsCancellationRequested)
				return document;

			ClassDeclarationSyntax modifiedDac = RemoveRegions(dacDeclaration, regionsVisitor.RegionNodesToRemove);
			var propertiesToRemove = modifiedDac.Members.OfType<PropertyDeclarationSyntax>() //-V3080
														.Where(p => identifierToRemove.Equals(p.Identifier.Text,
																							  StringComparison.OrdinalIgnoreCase));
			modifiedDac = modifiedDac.RemoveNodes(propertiesToRemove, SyntaxRemoveOptions.KeepExteriorTrivia);

			var dacFieldsToRemove = modifiedDac.Members.OfType<ClassDeclarationSyntax>()
													   .Where(dacField => identifierToRemove.Equals(dacField.Identifier.Text,
																									StringComparison.OrdinalIgnoreCase));
			modifiedDac = modifiedDac.RemoveNodes(dacFieldsToRemove, SyntaxRemoveOptions.KeepExteriorTrivia);
			var modifiedRoot = root.ReplaceNode(dacDeclaration, modifiedDac);

			if (cancellationToken.IsCancellationRequested)
				return document;

			//Format tabulations
			Workspace workspace = document.Project.Solution.Workspace;
			OptionSet formatOptions = GetFormattingOptions(workspace);
			modifiedRoot = Formatter.Format(modifiedRoot, workspace, formatOptions, cancellationToken);

			if (cancellationToken.IsCancellationRequested)
				return document;

			return document.WithSyntaxRoot(modifiedRoot);
		}

		private async Task<Document> MakeNonStatic(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode diagnosticNode = root?.FindNode(span);

			if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;

			ClassDeclarationSyntax dacDeclaration = diagnosticNode.Parent<ClassDeclarationSyntax>();
			string identifierToRemove = diagnosticNode is ClassDeclarationSyntax dacFieldDeclaration
											? dacFieldDeclaration.Identifier.Text
											: (diagnosticNode as PropertyDeclarationSyntax)?.Identifier.Text;

			if (identifierToRemove.IsNullOrWhiteSpace())
				return document;

			var regionsVisitor = new RegionsVisitor(identifierToRemove, cancellationToken);
			regionsVisitor.Visit(dacDeclaration);

			if (cancellationToken.IsCancellationRequested)
				return document;

			ClassDeclarationSyntax modifiedDac = RemoveRegions(dacDeclaration, regionsVisitor.RegionNodesToRemove);
			var propertiesToRemove = modifiedDac.Members.OfType<PropertyDeclarationSyntax>() //-V3080
														.Where(p => identifierToRemove.Equals(p.Identifier.Text,
																							  StringComparison.OrdinalIgnoreCase));
			modifiedDac = modifiedDac.RemoveNodes(propertiesToRemove, SyntaxRemoveOptions.KeepExteriorTrivia);

			var dacFieldsToRemove = modifiedDac.Members.OfType<ClassDeclarationSyntax>()
													   .Where(dacField => identifierToRemove.Equals(dacField.Identifier.Text,
																									StringComparison.OrdinalIgnoreCase));
			modifiedDac = modifiedDac.RemoveNodes(dacFieldsToRemove, SyntaxRemoveOptions.KeepExteriorTrivia);
			var modifiedRoot = root.ReplaceNode(dacDeclaration, modifiedDac);

			if (cancellationToken.IsCancellationRequested)
				return document;

			//Format tabulations
			Workspace workspace = document.Project.Solution.Workspace;
			OptionSet formatOptions = GetFormattingOptions(workspace);
			modifiedRoot = Formatter.Format(modifiedRoot, workspace, formatOptions, cancellationToken);

			if (cancellationToken.IsCancellationRequested)
				return document;

			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}