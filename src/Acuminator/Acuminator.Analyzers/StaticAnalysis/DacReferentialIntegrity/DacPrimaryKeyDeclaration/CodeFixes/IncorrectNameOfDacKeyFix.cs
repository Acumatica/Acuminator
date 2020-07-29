using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class IncorrectNameOfDacKeyFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableHashSet.Create
            (      
                Descriptors.PX1036_WrongDacPrimaryKeyName.Id,
                Descriptors.PX1036_WrongDacForeignKeyName.Id
            )
            .ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

            if (diagnostic == null || !diagnostic.Properties.TryGetValue(nameof(RefIntegrityDacKeyType), out string dacKeyTypeString) ||
                dacKeyTypeString.IsNullOrWhiteSpace() || !Enum.TryParse(dacKeyTypeString, out RefIntegrityDacKeyType dacKeyType))
            {
                return;
            }

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
											 .ConfigureAwait(false);

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
				when keyNode.Identifier.Text != TypeNames.UniqueKeyClassName:
					{
						var codeActionTitle = nameof(Resources.PX1036UKFix).GetLocalized().ToString();
						var codeAction = CodeAction.Create(codeActionTitle,
														   cancellation => ChangeKeyNameAsync(context.Document, root, keyNode, TypeNames.UniqueKeyClassName, cancellation),
														   equivalenceKey: codeActionTitle);

						context.RegisterCodeFix(codeAction, context.Diagnostics);
						return;
					}
				case RefIntegrityDacKeyType.ForeignKey:
                    //TODO add fix for foreign key 
					break;
			}     
		}

		private Task<Document> ChangeKeyNameAsync(Document document, SyntaxNode root, ClassDeclarationSyntax primaryKeyNode, string newKeyName,
												  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			var primaryKeyNodeWithNewName = primaryKeyNode.WithIdentifier(
																SyntaxFactory.Identifier(newKeyName));

			var newRoot = root.ReplaceNode(primaryKeyNode, primaryKeyNodeWithNewName);
			var newDocument = document.WithSyntaxRoot(newRoot);

			cancellation.ThrowIfCancellationRequested();

			return Task.FromResult(newDocument);			
		}
    }
}
