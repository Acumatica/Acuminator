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
using static Acuminator.Utilities.Roslyn.Constants.TypeNames;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class IncorrectDeclarationOfDacKeyFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			new[]
			{
				Descriptors.PX1036_WrongDacPrimaryKeyName.Id,
				Descriptors.PX1036_WrongDacForeignKeyDeclaration.Id,
				Descriptors.PX1036_WrongDacSingleUniqueKeyName.Id,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations.Id
			}
			.Distinct()
            .ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

            if (diagnostic == null || diagnostic.AdditionalLocations.IsNullOrEmpty() || diagnostic.AdditionalLocations[0] == null ||
				!diagnostic.IsRegisteredForCodeFix() || !diagnostic.Properties.TryGetValue(nameof(RefIntegrityDacKeyType), out string dacKeyTypeString) ||
                dacKeyTypeString.IsNullOrWhiteSpace() || !Enum.TryParse(dacKeyTypeString, out RefIntegrityDacKeyType dacKeyType))
            {
                return;
            }

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (!(root?.FindNode(context.Span) is ClassDeclarationSyntax keyNode))
				return;

			if (!(root.FindNode(diagnostic.AdditionalLocations[0].SourceSpan) is ClassDeclarationSyntax dacNode))
				return;

			switch (dacKeyType)
			{
				case RefIntegrityDacKeyType.PrimaryKey
				when keyNode.Identifier.Text != ReferentialIntegrity.PrimaryKeyClassName:
					{
						bool shouldChangeLocation = keyNode.Parent != dacNode;  //We need to change location for primary key
						string codeActionResourceName = shouldChangeLocation
							? nameof(Resources.PX1036PK_ChangeNameAndLocationFix)
							: nameof(Resources.PX1036PK_ChangeNameFix);

						var codeActionTitle = codeActionResourceName.GetLocalized().ToString();
						var codeAction = CodeAction.Create(codeActionTitle,
														   cancellation => ChangeKeyNameAsync(context.Document, dacNode, root, keyNode,
																							  ReferentialIntegrity.PrimaryKeyClassName,
																							  shouldChangeLocation, cancellation),
														   equivalenceKey: codeActionTitle);

						context.RegisterCodeFix(codeAction, context.Diagnostics);
						return;
					}
				case RefIntegrityDacKeyType.UniqueKey:
					RegisterCodeFixForUniqueKeys(context, dacNode, root, keyNode, diagnostic);
					return;

				case RefIntegrityDacKeyType.ForeignKey:
					{
						List<ClassDeclarationSyntax> keyNodesNotInFK = GetAllKeyNodesNotInContainer(diagnostic, root, keyNode);
						var codeActionTitle = nameof(Resources.PX1036FKFix).GetLocalized().ToString();
						var codeAction = CodeAction.Create(codeActionTitle,
														   cancellation => MultipleKeyNotInContainerDeclarationsFixAsync(context.Document, root, keyNodesNotInFK, dacNode,
																														 RefIntegrityDacKeyType.ForeignKey, cancellation),
														   equivalenceKey: codeActionTitle);

						context.RegisterCodeFix(codeAction, context.Diagnostics);
						return;
					}
			}
		}

		private void RegisterCodeFixForUniqueKeys(CodeFixContext context, ClassDeclarationSyntax dacNode, SyntaxNode root, ClassDeclarationSyntax keyNode, 
												  Diagnostic diagnostic)
		{
			bool isMultipleUniqueKeysFix = diagnostic.Properties.TryGetValue(nameof(UniqueKeyCodeFixType), out string uniqueCodeFixTypeString) &&
										   !uniqueCodeFixTypeString.IsNullOrWhiteSpace() &&
										   Enum.TryParse(uniqueCodeFixTypeString, out UniqueKeyCodeFixType uniqueCodeFixType) &&
										   uniqueCodeFixType == UniqueKeyCodeFixType.MultipleUniqueKeys;
			if (isMultipleUniqueKeysFix)
			{
				List<ClassDeclarationSyntax> keyNodesNotInUK = GetAllKeyNodesNotInContainer(diagnostic, root, keyNode);
				var codeActionTitle = nameof(Resources.PX1036MultipleUKFix).GetLocalized().ToString();
				var codeAction = CodeAction.Create(codeActionTitle,
												   cancellation => MultipleKeyNotInContainerDeclarationsFixAsync(context.Document, root, keyNodesNotInUK, dacNode,
																												 RefIntegrityDacKeyType.UniqueKey, cancellation),
												   equivalenceKey: codeActionTitle);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			}
			else if (keyNode.Identifier.Text != ReferentialIntegrity.UniqueKeyClassName)
			{
				var codeActionTitle = nameof(Resources.PX1036SingleUKFix).GetLocalized().ToString();
				var codeAction = CodeAction.Create(codeActionTitle,
												   cancellation => ChangeKeyNameAsync(context.Document, dacNode, root, keyNode,
																					  ReferentialIntegrity.UniqueKeyClassName,
																					  shouldChangeLocation: false, cancellation),
												   equivalenceKey: codeActionTitle);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			}
		}

		private List<ClassDeclarationSyntax> GetAllKeyNodesNotInContainer(Diagnostic diagnostic, SyntaxNode root, ClassDeclarationSyntax keyNode)
		{
			if (diagnostic.AdditionalLocations.Count == 1)
				return new List<ClassDeclarationSyntax>(capacity: 1) { keyNode };

			var otherKeysNotInContainerLocations = diagnostic.AdditionalLocations.Skip(1);  //First location is the location of DAC node
			var allKeyNodes = otherKeysNotInContainerLocations.Select(location => root.FindNode(location.SourceSpan))
															  .OfType<ClassDeclarationSyntax>()
															  .Concat(new[] { keyNode })
															  .OrderBy(node => node.SpanStart)
															  .ToList(capacity: diagnostic.AdditionalLocations.Count);
			return allKeyNodes;
		}

		private async Task<Document> ChangeKeyNameAsync(Document document, ClassDeclarationSyntax dacNode, SyntaxNode root, ClassDeclarationSyntax keyNode,
														string newKeyName, bool shouldChangeLocation, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();			
			ClassDeclarationSyntax keyNodeWithNewName = keyNode.WithIdentifier(Identifier(newKeyName));

			if (!shouldChangeLocation)
			{
				var newRoot = root.ReplaceNode(keyNode, keyNodeWithNewName);
				return document.WithSyntaxRoot(newRoot);
			}

			SyntaxNode trackingRoot = root.TrackNodes(dacNode, keyNode);
			keyNode = trackingRoot.GetCurrentNode(keyNode);

			if (keyNode == null)
				return document;

			trackingRoot = trackingRoot.RemoveNode(keyNode, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives);
			cancellation.ThrowIfCancellationRequested();
			dacNode = trackingRoot.GetCurrentNode(dacNode);

			if (dacNode == null)
				return document;

			var generator = SyntaxGenerator.GetGenerator(document);
			keyNodeWithNewName = keyNodeWithNewName.WithoutLeadingTrivia()
												   .WithTrailingTrivia(EndOfLine(Environment.NewLine), EndOfLine(Environment.NewLine))
												   .WithAdditionalAnnotations(Formatter.Annotation);

			var newDacNode = generator.InsertMembers(dacNode, 0, keyNodeWithNewName);
			trackingRoot = trackingRoot.ReplaceNode(dacNode, newDacNode);

			var newDocument = document.WithSyntaxRoot(trackingRoot);
			var formattedDocument = await Formatter.FormatAsync(newDocument, Formatter.Annotation, cancellationToken: cancellation)
												   .ConfigureAwait(false);
			return formattedDocument;
		}

		private async Task<Document> MultipleKeyNotInContainerDeclarationsFixAsync(Document document, SyntaxNode root, List<ClassDeclarationSyntax> keyNodesNotInContainer,
																				   ClassDeclarationSyntax dacNode, RefIntegrityDacKeyType keyType,
																				   CancellationToken cancellation)
		{
			string containerName = keyType switch
			{ 
				RefIntegrityDacKeyType.ForeignKey => ReferentialIntegrity.ForeignKeyClassName,
				RefIntegrityDacKeyType.UniqueKey  => ReferentialIntegrity.UniqueKeyClassName,
				_ => null
			};

			if (containerName == null)
				return document;

			var keysContainerNode = dacNode.Members.OfType<ClassDeclarationSyntax>()
												   .FirstOrDefault(nestedType => nestedType.Identifier.Text == containerName);

			SyntaxNode changedRoot = keysContainerNode != null
				? PlaceKeysIntoExistingContainer(root, keyNodesNotInContainer, keysContainerNode, cancellation)
				: PlaceKeysIntoNewKeysContainer(document, root, keyNodesNotInContainer, containerName, dacNode, cancellation);

			var newDocument = document.WithSyntaxRoot(changedRoot);
			var formattedDocument = await Formatter.FormatAsync(newDocument, Formatter.Annotation, cancellationToken: cancellation)
												   .ConfigureAwait(false);
			return formattedDocument;
		}

		private SyntaxNode PlaceKeysIntoExistingContainer(SyntaxNode root, List<ClassDeclarationSyntax> keyNodesNotInContainer, 
														  ClassDeclarationSyntax keysContainerNode, CancellationToken cancellation)
		{
			var nodesToTrack = keyNodesNotInContainer.Concat(keysContainerNode.ToEnumerable());
			SyntaxNode trackingRoot = root.TrackNodes(nodesToTrack);
			var keyNodesInChangedTree = keyNodesNotInContainer.Select(keyNode => trackingRoot.GetCurrentNode(keyNode))
															  .Where(keyNode => keyNode != null)
															  .ToList(capacity: keyNodesNotInContainer.Count);
			if (keyNodesInChangedTree.Count == 0)
				return root;

			trackingRoot = trackingRoot.RemoveNodes(keyNodesInChangedTree, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives);	
			
			var keysContainerNodeInChangedTree = trackingRoot.GetCurrentNode(keysContainerNode);

			if (keysContainerNodeInChangedTree == null)
				return root;

			cancellation.ThrowIfCancellationRequested();

			var newMembersList = keysContainerNodeInChangedTree.Members.InsertRange(index: keysContainerNodeInChangedTree.Members.Count, 
																					keyNodesNotInContainer.Select(RemoveStructuredTriviaFromKeyNode));
			var keysContainerNodeWithAddedKeys = keysContainerNodeInChangedTree.WithMembers(newMembersList)
																			   .WithAdditionalAnnotations(Formatter.Annotation);

			return trackingRoot.ReplaceNode(keysContainerNodeInChangedTree, keysContainerNodeWithAddedKeys);
		}

		private SyntaxNode PlaceKeysIntoNewKeysContainer(Document document, SyntaxNode root, List<ClassDeclarationSyntax> keyNodesNotInContainer,
														 string containerName, ClassDeclarationSyntax dacNode, CancellationToken cancellation)
		{
			var nodesToTrack = keyNodesNotInContainer.Concat(dacNode.ToEnumerable());
			SyntaxNode trackingRoot = root.TrackNodes(nodesToTrack);
			dacNode = trackingRoot.GetCurrentNode(dacNode);

			if (dacNode == null)
				return root;

			int positionToInsertKeysContainer = containerName == ReferentialIntegrity.UniqueKeyClassName
				? GetPositionToInsertUKContainer(dacNode)
				: GetPositionToInsertFKContainer(dacNode);

			var generator = SyntaxGenerator.GetGenerator(document);
			var keysContainerNode = CreateKeysContainerClassNode(generator, containerName, keyNodesNotInContainer)
															.WithAdditionalAnnotations(Formatter.Annotation);
			var newDacNode = generator.InsertMembers(dacNode, positionToInsertKeysContainer, keysContainerNode);

			cancellation.ThrowIfCancellationRequested();

			trackingRoot = trackingRoot.ReplaceNode(dacNode, newDacNode);
			var keyNodesInChangedTree = keyNodesNotInContainer.Select(keyNode => trackingRoot.GetCurrentNode(keyNode))
															  .Where(keyNode => keyNode != null)
															  .ToList(capacity: keyNodesNotInContainer.Count);
			if (keyNodesInChangedTree.Count == 0)
				return root;

			return trackingRoot.RemoveNodes(keyNodesInChangedTree, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives);
		}

		private int GetPositionToInsertUKContainer(ClassDeclarationSyntax dacNode)
		{
			int position = 0;
			
			for (int i = 0; i < dacNode.Members.Count; i++)
			{
				if ((dacNode.Members[i] is ClassDeclarationSyntax nestedTypeNode) && nestedTypeNode.Identifier.Text == ReferentialIntegrity.PrimaryKeyClassName)
					return i + 1;
			}

			return position;
		}

		private int GetPositionToInsertFKContainer(ClassDeclarationSyntax dacNode)
		{
			int primaryKeyPosition = -1;

			for (int i = 0; i < dacNode.Members.Count; i++)
			{
				if (!(dacNode.Members[i] is ClassDeclarationSyntax nestedTypeNode))
					continue;

				if (nestedTypeNode.Identifier.Text == ReferentialIntegrity.PrimaryKeyClassName)
					primaryKeyPosition = i;
				else if (nestedTypeNode.Identifier.Text == ReferentialIntegrity.UniqueKeyClassName)
					return i + 1;
			}

			return primaryKeyPosition + 1;
		}

		private ClassDeclarationSyntax CreateKeysContainerClassNode(SyntaxGenerator generator, string containerName, 
																	List<ClassDeclarationSyntax> keyNodesNotInContainer)
		{
			ClassDeclarationSyntax containerClassDeclaration =
				generator.ClassDeclaration(containerName, typeParameters: null,
										   Accessibility.Public, DeclarationModifiers.Static,
										   members: keyNodesNotInContainer.Select(RemoveStructuredTriviaFromKeyNode)) as ClassDeclarationSyntax;

			return containerClassDeclaration.WithTrailingTrivia(EndOfLine(Environment.NewLine));
		}

		private ClassDeclarationSyntax RemoveStructuredTriviaFromKeyNode(ClassDeclarationSyntax keyNode)
		{
			if (!keyNode.HasStructuredTrivia)
				return keyNode;

			var nonStructuredLeadingTrivia = keyNode.GetLeadingTrivia()
													.Where(trivia => !trivia.HasStructure);
			var nonStructuredTrailingTrivia = keyNode.GetTrailingTrivia()
													 .Where(trivia => !trivia.HasStructure);

			return keyNode.WithLeadingTrivia(nonStructuredLeadingTrivia)
						  .WithTrailingTrivia(nonStructuredTrailingTrivia);
		}
	}
}
