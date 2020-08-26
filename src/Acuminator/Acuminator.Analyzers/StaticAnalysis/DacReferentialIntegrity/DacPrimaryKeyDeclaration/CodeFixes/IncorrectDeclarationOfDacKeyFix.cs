using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class IncorrectDeclarationOfDacKeyFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableHashSet.Create
            (      
                Descriptors.PX1036_WrongDacPrimaryKeyName.Id,
                Descriptors.PX1036_WrongDacForeignKeyName.Id,
				Descriptors.PX1036_WrongDacSingleUniqueKeyName.Id,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.Id
            )
            .ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

            if (diagnostic == null || !diagnostic.IsRegisteredForCodeFix() || 
				!diagnostic.Properties.TryGetValue(nameof(RefIntegrityDacKeyType), out string dacKeyTypeString) ||
                dacKeyTypeString.IsNullOrWhiteSpace() || !Enum.TryParse(dacKeyTypeString, out RefIntegrityDacKeyType dacKeyType))
            {
                return;
            }

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (!(root?.FindNode(context.Span) is ClassDeclarationSyntax keyNode))
				return;

			switch (dacKeyType)
			{
				case RefIntegrityDacKeyType.PrimaryKey 
				when keyNode.Identifier.Text != TypeNames.PrimaryKeyClassName:
					{
						var codeActionTitle = nameof(Resources.PX1036PKFix).GetLocalized().ToString();
						var codeAction = CodeAction.Create(codeActionTitle,
														   cancellation => ChangeKeyNameAsync(context.Document, root, keyNode, TypeNames.PrimaryKeyClassName, cancellation),
														   equivalenceKey: codeActionTitle);

						context.RegisterCodeFix(codeAction, context.Diagnostics);
						return;
					}
				case RefIntegrityDacKeyType.UniqueKey:
					RegisterCodeFixForUniqueKeys(context, root, keyNode, diagnostic);
					return;

				case RefIntegrityDacKeyType.ForeignKey:
                    //TODO add fix for foreign key 
					break;
			}     
		}

		private void RegisterCodeFixForUniqueKeys(CodeFixContext context, SyntaxNode root, ClassDeclarationSyntax keyNode, Diagnostic diagnostic)
		{
			bool isMultipleUniqueKeysFix = diagnostic.Properties.TryGetValue(nameof(UniqueKeyCodeFixType), out string uniqueCodeFixTypeString) &&
										   !uniqueCodeFixTypeString.IsNullOrWhiteSpace() &&
										   Enum.TryParse(uniqueCodeFixTypeString, out UniqueKeyCodeFixType uniqueCodeFixType) &&
										   uniqueCodeFixType == UniqueKeyCodeFixType.MultipleUniqueKeys;
			if (isMultipleUniqueKeysFix)
			{
				if (diagnostic.AdditionalLocations.IsNullOrEmpty() || diagnostic.AdditionalLocations[0] == null)
					return;

				if (!(root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan) is ClassDeclarationSyntax dacNode))
					return;

				var codeActionTitle = nameof(Resources.PX1036MultipleUKFix).GetLocalized().ToString();
				var codeAction = CodeAction.Create(codeActionTitle,
												   cancellation => MultipleUniqueKeyDeclarationsFixAsync(context.Document, root, keyNode, dacNode, cancellation),
												   equivalenceKey: codeActionTitle);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			}
			else if (keyNode.Identifier.Text != TypeNames.UniqueKeyClassName)
			{
				var codeActionTitle = nameof(Resources.PX1036SingleUKFix).GetLocalized().ToString();
				var codeAction = CodeAction.Create(codeActionTitle,
												   cancellation => ChangeKeyNameAsync(context.Document, root, keyNode, TypeNames.UniqueKeyClassName, cancellation),
												   equivalenceKey: codeActionTitle);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			}
		}

		private Task<Document> ChangeKeyNameAsync(Document document, SyntaxNode root, ClassDeclarationSyntax keyNode, string newKeyName,
												  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var keyNodeWithNewName = keyNode.WithIdentifier(
													SyntaxFactory.Identifier(newKeyName));

			var newRoot = root.ReplaceNode(keyNode, keyNodeWithNewName);
			var newDocument = document.WithSyntaxRoot(newRoot);

			cancellation.ThrowIfCancellationRequested();

			return Task.FromResult(newDocument);			
		}

		private async Task<Document> MultipleUniqueKeyDeclarationsFixAsync(Document document, SyntaxNode root, ClassDeclarationSyntax keyNode,
																		   ClassDeclarationSyntax dacNode, CancellationToken cancellation)
		{
			var uniqueKeysContainer = dacNode.Members.OfType<ClassDeclarationSyntax>()
													 .FirstOrDefault(nestedType => nestedType.Identifier.Text == TypeNames.UniqueKeyClassName);

			SyntaxNode changedRoot = uniqueKeysContainer != null
				? PlaceUniqueKeyIntoExistingContainer(root, keyNode, uniqueKeysContainer, cancellation)
				: PlaceUniqueKeyIntoNewUniqueKeysContainer(document, root, keyNode, dacNode, cancellation);

			var newDocument = document.WithSyntaxRoot(changedRoot);
			var formattedDocument = await Formatter.FormatAsync(newDocument, Formatter.Annotation, cancellationToken: cancellation)
												   .ConfigureAwait(false);
			return formattedDocument;
		}

		private SyntaxNode PlaceUniqueKeyIntoExistingContainer(SyntaxNode root, ClassDeclarationSyntax keyNode, ClassDeclarationSyntax uniqueKeysContainer,
															   CancellationToken cancellation)
		{
			SyntaxNode trackingRoot = root.TrackNodes(uniqueKeysContainer, keyNode);
			keyNode = trackingRoot.GetCurrentNode(keyNode);

			if (keyNode == null)
				return root;

			trackingRoot = trackingRoot.RemoveNode(keyNode, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives);
			var uniqueKeysContainerInChangedTree = trackingRoot.GetCurrentNode(uniqueKeysContainer);

			if (uniqueKeysContainerInChangedTree == null)
				return root;

			cancellation.ThrowIfCancellationRequested();

			var newMembersList = uniqueKeysContainerInChangedTree.Members.Insert(index: uniqueKeysContainerInChangedTree.Members.Count, keyNode);
			var uniqueKeysContainerWithUniqueKey = uniqueKeysContainerInChangedTree.WithMembers(newMembersList)
																				   .WithAdditionalAnnotations(Formatter.Annotation);

			return trackingRoot.ReplaceNode(uniqueKeysContainerInChangedTree, uniqueKeysContainerWithUniqueKey);
		}

		private SyntaxNode PlaceUniqueKeyIntoNewUniqueKeysContainer(Document document, SyntaxNode root, ClassDeclarationSyntax keyNode, 
																	ClassDeclarationSyntax dacNode, CancellationToken cancellation)
		{
			SyntaxNode trackingRoot = root.TrackNodes(dacNode, keyNode);
			dacNode = trackingRoot.GetCurrentNode(dacNode);

			if (dacNode == null)
				return root;

			int positionToInsertUK = GetPositionToInsertUKContainer(dacNode);
			var generator = SyntaxGenerator.GetGenerator(document);
			var uniqueKeyContainerNode = CreateUniqueKeysContainerClassNode(generator, keyNode)
											.WithAdditionalAnnotations(Formatter.Annotation);
			var newDacNode = generator.InsertMembers(dacNode, positionToInsertUK, uniqueKeyContainerNode);

			cancellation.ThrowIfCancellationRequested();

			trackingRoot = trackingRoot.ReplaceNode(dacNode, newDacNode);
			var keyNodeInChangedTree = trackingRoot.GetCurrentNode(keyNode);

			if (keyNodeInChangedTree == null)
				return root;

			return trackingRoot.RemoveNode(keyNodeInChangedTree, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives);
		}

		private int GetPositionToInsertUKContainer(ClassDeclarationSyntax dacNode)
		{
			int position = 0;
			
			for (int i = 0; i < dacNode.Members.Count; i++)
			{
				if ((dacNode.Members[i] is ClassDeclarationSyntax nestedTypeNode) && nestedTypeNode.Identifier.Text == TypeNames.PrimaryKeyClassName)
					return i + 1;
			}

			return position;
		}

		private ClassDeclarationSyntax CreateUniqueKeysContainerClassNode(SyntaxGenerator generator, ClassDeclarationSyntax keyNode)
		{
			ClassDeclarationSyntax ukClassDeclaration = 
				generator.ClassDeclaration(TypeNames.UniqueKeyClassName, typeParameters: null, 
										   Accessibility.Public, DeclarationModifiers.Static, 
										   members: keyNode.ToEnumerable()) as ClassDeclarationSyntax;

			return ukClassDeclaration.WithTrailingTrivia(EndOfLine(Environment.NewLine));
		}
	}
}
