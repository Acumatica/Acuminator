﻿using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlConstant
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class LegacyBqlConstantFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1061_LegacyBqlConstant.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var classNode = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			if (classNode == null)
				return;

			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1061_LegacyBqlConstant.Id);
			if (diagnostic == null)
				return;

			if (diagnostic.Properties == null || !diagnostic.Properties.TryGetValue(LegacyBqlConstantAnalyzer.CorrespondingType, out string typeName) ||
				typeName.IsNullOrEmpty() || context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			SimpleBaseTypeSyntax newBaseType = CreateBaseType(typeName, classNode.Identifier.Text);

			if (newBaseType != null)
			{
				string title = nameof(Resources.PX1061Fix).GetLocalized().ToString();
				context.RegisterCodeFix(
					CodeAction.Create(title,
									  c => Task.FromResult(GetDocumentWithUpdatedBqlField(context.Document, root, classNode, newBaseType)),
									  equivalenceKey: title),
					context.Diagnostics);
			}
		}

		private SimpleBaseTypeSyntax CreateBaseType(string typeName, string constantName)
		{
			if (!LegacyBqlFieldAnalyzer.PropertyTypeToFieldType.ContainsKey(typeName))
				return null;

			string bqlTypeName = $"Bql{LegacyBqlFieldAnalyzer.PropertyTypeToFieldType[typeName]}";
			GenericNameSyntax constantTypeNode =
				GenericName(Identifier("Constant"))
					.WithTypeArgumentList(
						TypeArgumentList(
							SingletonSeparatedList<TypeSyntax>(IdentifierName(constantName))));

			var newBaseType =
				SimpleBaseType(
					QualifiedName(
						QualifiedName(
							QualifiedName(
								QualifiedName(
									IdentifierName("PX"),
									IdentifierName("Data")),
									IdentifierName("BQL")),
									IdentifierName(bqlTypeName)),
						constantTypeNode));

			return newBaseType;
		}

		private Document GetDocumentWithUpdatedBqlField(Document oldDocument, SyntaxNode root, ClassDeclarationSyntax classNode, SimpleBaseTypeSyntax newBaseType)
		{
			var newClassNode =
				classNode.WithBaseList(
					BaseList(
						SingletonSeparatedList<BaseTypeSyntax>(newBaseType)));
			var newRoot = root.ReplaceNode(classNode, newClassNode);
			return oldDocument.WithSyntaxRoot(newRoot);
		}
	}
}