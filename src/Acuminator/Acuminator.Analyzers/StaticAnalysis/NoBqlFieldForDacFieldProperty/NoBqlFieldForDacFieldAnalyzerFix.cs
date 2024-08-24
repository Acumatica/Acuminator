﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.CodeGeneration;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class NoBqlFieldForDacFieldAnalyzerFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1065_NoBqlFieldForDacFieldProperty.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var diagnostics = context.Diagnostics;

			if (diagnostics.IsDefaultOrEmpty)
				return Task.CompletedTask;
			else if (diagnostics.Length == 1)
			{
				var diagnostic = diagnostics[0];

				if (diagnostic.Id != Descriptors.PX1065_NoBqlFieldForDacFieldProperty.Id)
					return Task.CompletedTask;

				return RegisterCodeFixeForDiagnosticAsync(context, diagnostic);
			}

			List<Task> allTasks = new(capacity: diagnostics.Length);

			foreach (Diagnostic diagnostic in context.Diagnostics)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (diagnostic.Id != Descriptors.PX1065_NoBqlFieldForDacFieldProperty.Id)
					continue;

				var task = RegisterCodeFixeForDiagnosticAsync(context, diagnostic);
				allTasks.Add(task);
			}

			return Task.WhenAll(allTasks);
		}

		private Task RegisterCodeFixeForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.IsRegisteredForCodeFix() ||
				!diagnostic.TryGetPropertyValue(DiagnosticProperty.DacFieldName, out string? dacFieldame) ||
				dacFieldame.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.PropertyType, out string? propertyType) ||
				propertyType.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			string bqlFieldName = dacFieldame.FirstCharToLower();

			// equivalence key should not contain format arguments to allow mass code fixes
			string equivalenceKey = nameof(Resources.PX1065FixFormat).GetLocalized().ToString();
			string codeActionName = nameof(Resources.PX1065FixFormat).GetLocalized(bqlFieldName).ToString();
			var codeAction = CodeAction.Create(codeActionName,
											   cToken => AddBqlFieldAsync(context.Document, bqlFieldName, propertyType,
																		  context.Span, cToken),
											   equivalenceKey);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private async Task<Document> AddBqlFieldAsync(Document document, string bqlFieldName, string propertyType, TextSpan span,
													  CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? nodeWithDiagnostic = root?.FindNode(span);
			var propertyWithoutBqlFieldNode = (nodeWithDiagnostic as PropertyDeclarationSyntax) ??
											   nodeWithDiagnostic?.Parent<PropertyDeclarationSyntax>();
			if (propertyWithoutBqlFieldNode == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var dacNode = propertyWithoutBqlFieldNode.Parent<ClassDeclarationSyntax>();

			if (dacNode == null)
				return document;

			var newDacMembers = CreateMembersListWithBqlField(dacNode, propertyWithoutBqlFieldNode, bqlFieldName, propertyType);

			if (newDacMembers == null)
				return document;

			var newDacNode = dacNode.WithMembers(newDacMembers.Value);
			var newRoot = root.ReplaceNode(dacNode, newDacNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private SyntaxList<MemberDeclarationSyntax>? CreateMembersListWithBqlField(ClassDeclarationSyntax dacNode, 
																				   PropertyDeclarationSyntax propertyWithoutBqlFieldNode,
																				   string bqlFieldName, string propertyType)
		{
			var members = dacNode.Members;

			if (members.Count == 0)
			{
				var newSingleBqlFieldNode = CodeGeneration.GenerateBqlField(propertyWithoutBqlFieldNode, propertyType, 
																			bqlFieldName, isFirstField: true);
				return newSingleBqlFieldNode != null
					? SingletonList<MemberDeclarationSyntax>(newSingleBqlFieldNode)
					: null;
			}

			int propertyMemberIndex = dacNode.Members.IndexOf(propertyWithoutBqlFieldNode);

			if (propertyMemberIndex < 0)
				propertyMemberIndex = 0;

			var newBqlFieldNode = CodeGeneration.GenerateBqlField(propertyWithoutBqlFieldNode, propertyType, bqlFieldName, 
																  isFirstField: propertyMemberIndex == 0);
			var propertyWithoutRegions = RemoveRegionsFromPropertyTrivia(propertyWithoutBqlFieldNode);
			var newMembers = members.Replace(propertyWithoutBqlFieldNode, propertyWithoutRegions)
									.Insert(propertyMemberIndex, newBqlFieldNode);
			return newMembers;
		}

		private PropertyDeclarationSyntax RemoveRegionsFromPropertyTrivia(PropertyDeclarationSyntax property)
		{
			var leadingTrivia = property.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return property;

			var regionTrivias = leadingTrivia.GetRegionDirectiveLinesFromTrivia();

			if (regionTrivias.Count == 0)
				return property;

			var newLeadingTrivia = leadingTrivia.Except(regionTrivias);
			return property.WithLeadingTrivia(newLeadingTrivia);
		}
	}
}