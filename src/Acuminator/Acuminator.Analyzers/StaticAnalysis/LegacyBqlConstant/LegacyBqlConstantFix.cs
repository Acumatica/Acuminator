using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlConstant
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class LegacyBqlConstantFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1061_LegacyBqlConstant.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var constantNode = root?.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			if (constantNode == null)
				return;

			if (!diagnostic.TryGetPropertyValue(LegacyBqlConstantAnalyzer.CorrespondingType, out string? propertyTypeName) ||
				propertyTypeName.IsNullOrWhiteSpace())
			{
				return;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			var strongPropertyTypeName = new PropertyTypeName(propertyTypeName);
			SimpleBaseTypeSyntax? newBaseType = CreateBaseType(strongPropertyTypeName, constantNode.Identifier.Text);

			if (newBaseType != null)
			{
				string title = nameof(Resources.PX1061Fix).GetLocalized().ToString();
				context.RegisterCodeFix(
					CodeAction.Create(title,
									  c => Task.FromResult(GetDocumentWithUpdatedBqlField(context.Document, root!, constantNode, newBaseType)),
									  equivalenceKey: title),
					diagnostic);
			}
		}

		private SimpleBaseTypeSyntax? CreateBaseType(PropertyTypeName propertyTypeName, string constantName)
		{
			string? bqlTypeName = PropertyTypeToBqlFieldTypeMapping.GetBqlFieldType(propertyTypeName).NullIfWhiteSpace();

			if (bqlTypeName == null)
				return null;

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