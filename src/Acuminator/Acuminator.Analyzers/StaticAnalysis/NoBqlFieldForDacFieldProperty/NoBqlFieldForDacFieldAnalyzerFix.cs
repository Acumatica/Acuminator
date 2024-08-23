using System;
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
			string codeActionName = nameof(Resources.PX1065FixFormat).GetLocalized(bqlFieldName).ToString();
			var codeAction = CodeAction.Create(codeActionName,
											   cToken => AddBqlFieldAsync(context.Document, bqlFieldName, propertyType,
																		  context.Span, cToken),
											   equivalenceKey: codeActionName);
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

			var newBqlFieldNode = CodeGeneration.GenerateBqlField(propertyType, bqlFieldName);
			
			if (newBqlFieldNode == null)
				return document;

			var newDacMembers = CreateMembersListWithBqlField(dacNode, propertyWithoutBqlFieldNode, newBqlFieldNode);

			if (newDacMembers == null)
				return document;

			var newDacNode = dacNode.WithMembers(newDacMembers.Value);
			var newRoot = root.ReplaceNode(dacNode, newDacNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private SyntaxList<MemberDeclarationSyntax>? CreateMembersListWithBqlField(ClassDeclarationSyntax dacNode, 
																				   PropertyDeclarationSyntax propertyWithoutBqlFieldNode,
																				   ClassDeclarationSyntax newBqlFieldNode)
		{
			var members = dacNode.Members;

			if (members.Count == 0)
				return SingletonList<MemberDeclarationSyntax>(newBqlFieldNode);

			int propertyMemberIndex = dacNode.Members.IndexOf(propertyWithoutBqlFieldNode);

			if (propertyMemberIndex < 0)
				propertyMemberIndex = 0;

			var newMembers = members.Insert(propertyMemberIndex, newBqlFieldNode);
			return newMembers;
		}
	}
}