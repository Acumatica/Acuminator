using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;
using PX.Data;

namespace Acuminator.Analyzers.FixProviders
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class DacExtensionDefaultAttributeFix : CodeFixProvider
	{
		private const string _PXUnboundDefaultAttributeName = "PXUnboundDefault";
		private const string _PXPersistingCheck = nameof(PXPersistingCheck);
		private const string _PersistingCheck = nameof(PXDefaultAttribute.PersistingCheck);
		private const string _PersistingCheckNothing = nameof(PXPersistingCheck.Nothing);


		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Id);

			if (diagnostic == null)
				return;

			if (diagnostic.IsBoundField())
			{
				string codeActionNameBound = nameof(Resources.PX1030FixBound).GetLocalized().ToString();
				CodeAction codeActionBound =
					CodeAction.Create(codeActionNameBound,
							cToken => ReplaceIncorrectDefaultAttribute(context.Document, context.Span, diagnostic.IsBoundField(), cToken),
							equivalenceKey: codeActionNameBound);

				context.RegisterCodeFix(codeActionBound, context.Diagnostics);
			}
			else
			{
				string codeActionNameUnbound = nameof(Resources.PX1030FixUnbound).GetLocalized().ToString();
				CodeAction codeActionUnbound =
					CodeAction.Create(codeActionNameUnbound,
										cToken => ReplaceIncorrectDefaultAttribute(context.Document, context.Span, diagnostic.IsBoundField(), cToken),
										equivalenceKey: codeActionNameUnbound);

				context.RegisterCodeFix(codeActionUnbound, context.Diagnostics);
			}
		}

		private async Task<Document> ReplaceIncorrectDefaultAttribute(Document document, TextSpan span, bool isBoundField,CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			 if (!(root?.FindNode(span) is AttributeSyntax attributeNode))
				return document;

			if (!(attributeNode.Parent is AttributeListSyntax attributeList))
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);

			var memberAccessExpression = generator.MemberAccessExpression(generator.IdentifierName(_PXPersistingCheck),
																		  generator.IdentifierName(_PersistingCheckNothing));
			var persistingAttributeArgument = generator.AttributeArgument(_PersistingCheck, 
																		  memberAccessExpression) as AttributeArgumentSyntax;
			var pxUnboundDefaultAttribute = generator.Attribute(_PXUnboundDefaultAttributeName) as AttributeListSyntax;
			
			SyntaxNode modifiedRoot;

			if (isBoundField)
			{
				if (attributeNode.ArgumentList != null)
				{
					var persistingCheckArguments = from arguments in attributeNode.ArgumentList.Arguments
													where arguments.GetText().ToString().Contains(_PersistingCheck)
													select arguments;
					
					if (persistingCheckArguments.Any())
					{
						AttributeArgumentSyntax argument = persistingCheckArguments.First();
						persistingAttributeArgument = argument.ReplaceNode(argument.Expression, memberAccessExpression);
						var newAttributeNode = attributeNode.ReplaceNode(argument, persistingAttributeArgument);
						var newAttributeList = attributeList.ReplaceNode(attributeNode, newAttributeNode);
						modifiedRoot = root.ReplaceNode(attributeList, newAttributeList);
					}
					else
					{
						var newAttributeList = generator.AddAttributeArguments(attributeNode, new SyntaxNode[] { persistingAttributeArgument }) as AttributeListSyntax;
						modifiedRoot = root.ReplaceNode(attributeNode, newAttributeList.Attributes[0]);
					}
				}
				else
				{
					AttributeListSyntax newAttribute = generator.InsertAttributeArguments(attributeNode, 1, new SyntaxNode[] { persistingAttributeArgument }) as AttributeListSyntax;
					modifiedRoot = root.ReplaceNode(attributeNode, newAttribute.Attributes[0]);
				}
			}
			else
			{
				modifiedRoot = root.ReplaceNode(attributeNode, pxUnboundDefaultAttribute.Attributes[0]);
			}

			return document.WithSyntaxRoot(modifiedRoot);
		}


	}

	internal static class DiagnosticHelper {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBoundField(this Diagnostic diagnostic)
		{
			diagnostic.ThrowOnNull(nameof(diagnostic));

			return diagnostic.Properties.TryGetValue(DiagnosticProperty.IsBoundField, out string boundFlag)
				? boundFlag == bool.TrueString
				: false;
		}
	}
}