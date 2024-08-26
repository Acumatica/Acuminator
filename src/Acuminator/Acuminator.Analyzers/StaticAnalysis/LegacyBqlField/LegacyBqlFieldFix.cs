using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.CodeGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlField
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class LegacyBqlFieldFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1060_LegacyBqlField.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
											 .ConfigureAwait(false);

			var bqlFieldNode = root?.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			if (bqlFieldNode == null)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.PropertyType, out string? propertyTypeName) || 
				propertyTypeName.IsNullOrWhiteSpace())
			{
				return;
			}

			string bqlFieldName = bqlFieldNode.Identifier.Text;
			SimpleBaseTypeSyntax? newBaseType = CodeGeneration.BaseTypeForBqlField(propertyTypeName, bqlFieldName);
			if (newBaseType == null)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();

			string title = nameof(Resources.PX1060Fix).GetLocalized().ToString();
			context.RegisterCodeFix(
				CodeAction.Create(title,
								  c => Task.FromResult(GetDocumentWithUpdatedBqlField(context.Document, root!, bqlFieldNode, newBaseType)),
								  equivalenceKey: title),
				context.Diagnostics); 
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