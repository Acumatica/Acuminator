﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public partial class ForbiddenFieldsInDacFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.Id,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			if (!diagnostic.IsRegisteredForCodeFix())
				return Task.CompletedTask;
			
			context.CancellationToken.ThrowIfCancellationRequested();

			string codeActionName = nameof(Resources.PX1027ForbiddenFieldsFix).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName,
													  cToken => DeleteForbiddenFieldsAsync(context.Document, context.Span, cToken),
													  equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);

			return Task.CompletedTask;
		}

		private async Task<Document> DeleteForbiddenFieldsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? diagnosticNode = root?.FindNode(span);

			if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;

			ClassDeclarationSyntax? dacDeclaration = diagnosticNode.Parent<ClassDeclarationSyntax>();
			string? identifierToRemove = diagnosticNode is ClassDeclarationSyntax dacFieldDeclaration
											? dacFieldDeclaration.Identifier.Text
											: (diagnosticNode as PropertyDeclarationSyntax)?.Identifier.Text;

			if (dacDeclaration == null || identifierToRemove.IsNullOrWhiteSpace())
				return document;

			var regionsVisitor = new RegionsVisitor(identifierToRemove, cancellationToken);
			regionsVisitor.Visit(dacDeclaration);

			if (cancellationToken.IsCancellationRequested)
				return document;

			ClassDeclarationSyntax modifiedDac = RemoveRegions(dacDeclaration, regionsVisitor.RegionNodesToRemove);
			var propertiesToRemove = modifiedDac.Members.OfType<PropertyDeclarationSyntax>() //-V3080
														.Where(p => identifierToRemove.Equals(p.Identifier.Text, 
																							  StringComparison.OrdinalIgnoreCase));
			modifiedDac = modifiedDac.RemoveNodes(propertiesToRemove, SyntaxRemoveOptions.KeepExteriorTrivia)!;

			var dacFieldsToRemove = modifiedDac.Members.OfType<ClassDeclarationSyntax>()
													   .Where(dacField => identifierToRemove.Equals(dacField.Identifier.Text, 
																									StringComparison.OrdinalIgnoreCase));
			modifiedDac = modifiedDac.RemoveNodes(dacFieldsToRemove, SyntaxRemoveOptions.KeepExteriorTrivia)!;
			var modifiedRoot = root!.ReplaceNode(dacDeclaration, modifiedDac);

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

		private ClassDeclarationSyntax RemoveRegions(ClassDeclarationSyntax dacDeclaration, List<DirectiveTriviaSyntax> regionNodesToRemove)
		{
			if (regionNodesToRemove.Count == 0)
				return dacDeclaration;

			var trackingDacDeclaration = dacDeclaration.TrackNodes(regionNodesToRemove);

			if (trackingDacDeclaration == null) 
				return dacDeclaration;

			foreach (DirectiveTriviaSyntax regionDirective in regionNodesToRemove)
			{
				SyntaxNode? regionInModifiedDeclaration = trackingDacDeclaration.GetCurrentNode(regionDirective);

				if (regionInModifiedDeclaration == null)
					continue;

				SyntaxToken parentToken = regionInModifiedDeclaration.ParentTrivia.Token;
				SyntaxToken newParentToken;
				int regionIndex = parentToken.LeadingTrivia.IndexOf(regionInModifiedDeclaration.ParentTrivia);

				if (regionIndex >= 0)
				{
					var newTrivia = RemoveRegionsFromTriviaList(parentToken.LeadingTrivia, regionInModifiedDeclaration.ParentTrivia, regionIndex); 
					newParentToken = parentToken.WithLeadingTrivia(newTrivia);
				}
				else
				{
					regionIndex = parentToken.TrailingTrivia.IndexOf(regionInModifiedDeclaration.ParentTrivia);

					if (regionIndex < 0)
						continue;

					var newTrivia = RemoveRegionsFromTriviaList(parentToken.TrailingTrivia, regionInModifiedDeclaration.ParentTrivia, regionIndex);
					newParentToken = parentToken.WithTrailingTrivia(newTrivia);
				}

				SyntaxNode? parentNode = regionInModifiedDeclaration.ParentTrivia.Token.Parent;
				SyntaxNode? newParentNode = parentNode?.ReplaceToken(parentToken, newParentToken);

				if (newParentNode != null)
					trackingDacDeclaration = trackingDacDeclaration.ReplaceNode(parentNode!, newParentNode);
			}

			return trackingDacDeclaration;
		}

		private SyntaxTriviaList RemoveRegionsFromTriviaList(SyntaxTriviaList trivias, SyntaxTrivia regionTriviaToRemove, int regionIndex)
		{
			var newTriviaList = trivias;
			
			if (regionIndex < trivias.Count - 1 && trivias[regionIndex + 1].IsKind(SyntaxKind.WhitespaceTrivia))
			{
				newTriviaList = newTriviaList.RemoveAt(regionIndex + 1);
			}

			newTriviaList = newTriviaList.RemoveAt(regionIndex);
			return newTriviaList;
		}


		private OptionSet GetFormattingOptions(Workspace workspace)
		{		
			int identationSize = workspace.Options.GetOption(FormattingOptions.IndentationSize, LanguageNames.CSharp);
			FormattingOptions.IndentStyle identationStyle = workspace.Options.GetOption(FormattingOptions.SmartIndent, LanguageNames.CSharp);
			int tabSize = workspace.Options.GetOption(FormattingOptions.TabSize, LanguageNames.CSharp);
			bool useTabs = workspace.Options.GetOption(FormattingOptions.UseTabs, LanguageNames.CSharp);

			return workspace.Options.WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, identationSize)
									.WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, identationStyle)
									.WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, tabSize)
									.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, useTabs);
		}
	}
}
