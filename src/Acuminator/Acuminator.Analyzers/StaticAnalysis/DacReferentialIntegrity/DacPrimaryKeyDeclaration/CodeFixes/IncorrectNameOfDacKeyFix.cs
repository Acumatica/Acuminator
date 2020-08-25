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

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class IncorrectNameOfDacKeyFix : CodeFixProvider
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
				case RefIntegrityDacKeyType.UniqueKey
				when diagnostic.Descriptor == Descriptors.PX1036_WrongDacSingleUniqueKeyName && 
					 keyNode.Identifier.Text != TypeNames.UniqueKeyClassName:
					{
						var codeActionTitle = nameof(Resources.PX1036SingleUKFix).GetLocalized().ToString();
						var codeAction = CodeAction.Create(codeActionTitle,
														   cancellation => ChangeKeyNameAsync(context.Document, root, keyNode, TypeNames.UniqueKeyClassName, cancellation),
														   equivalenceKey: codeActionTitle);

						context.RegisterCodeFix(codeAction, context.Diagnostics);
						return;
					}
				case RefIntegrityDacKeyType.UniqueKey
				when diagnostic.Descriptor == Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations:
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
						return;
					}
				case RefIntegrityDacKeyType.ForeignKey:
                    //TODO add fix for foreign key 
					break;
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

		private Task<Document> MultipleUniqueKeyDeclarationsFixAsync(Document document, SyntaxNode root, ClassDeclarationSyntax keyNode,
																	 ClassDeclarationSyntax dacNode, CancellationToken cancellation)
		{
			var uniqueKeysContainer = dacNode.Members.OfType<ClassDeclarationSyntax>()
													 .FirstOrDefault(nestedType => nestedType.Identifier.Text == TypeNames.UniqueKeyClassName);

			SyntaxNode changedRoot = uniqueKeysContainer != null
				? PlaceUniqueKeyIntoExistingContainer(root, keyNode, uniqueKeysContainer, cancellation)
				: PlaceUniqueKeyIntoNewUniqueKeysContainer(root, keyNode, dacNode, cancellation);
		
			cancellation.ThrowIfCancellationRequested();

			var newDocument = document.WithSyntaxRoot(changedRoot);
			return Task.FromResult(newDocument);
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

			var uniqueKeysContainerWithUniqueKey =
				uniqueKeysContainerInChangedTree.WithMembers(
						uniqueKeysContainerInChangedTree.Members.Insert(uniqueKeysContainerInChangedTree.Members.Count, keyNode));

			return trackingRoot.ReplaceNode(uniqueKeysContainerInChangedTree, uniqueKeysContainerWithUniqueKey);
		}

		private SyntaxNode PlaceUniqueKeyIntoNewUniqueKeysContainer(SyntaxNode root, ClassDeclarationSyntax keyNode, ClassDeclarationSyntax dacNode, 
																	CancellationToken cancellation)
		{
			SyntaxNode trackingRoot = root.TrackNodes(dacNode, keyNode);
			dacNode = trackingRoot.GetCurrentNode(dacNode);

			if (dacNode == null)
				return root;

			int positionToInsertUK = GetPositionToInsertUKContainer(dacNode);
			var uniqueKeyContainerNode = CreateUniqueKeysContainerClassNode(keyNode, cancellation);
			var newDacNode = dacNode.WithMembers(
										dacNode.Members.Insert(positionToInsertUK, uniqueKeyContainerNode));

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

		private ClassDeclarationSyntax CreateUniqueKeysContainerClassNode(ClassDeclarationSyntax keyNode, CancellationToken cancellation)
		{
			ClassDeclarationSyntax ukClassDeclaration =
				ClassDeclaration(TypeNames.ForeignKeyClassName)
					.WithModifiers(
						TokenList(
							new[]{
								Token(SyntaxKind.PublicKeyword),
								Token(SyntaxKind.StaticKeyword)}))
					.WithMembers(
						SingletonList<MemberDeclarationSyntax>(keyNode))
					.WithTrailingTrivia(EndOfLine(Environment.NewLine), EndOfLine(Environment.NewLine));

			return ukClassDeclaration;
		}
	}
}
