using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
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

			if (diagnostic.Properties == null
				|| !diagnostic.Properties.TryGetValue(LegacyBqlConstantAnalyzer.CorrespondingType, out string typeName)
				|| String.IsNullOrEmpty(typeName))
			{
				return;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			TypeSyntax newBaseType = CreateBaseType(typeName, classNode.Identifier.Text);

			if (newBaseType != null)
			{
				string title = nameof(Resources.PX1061Fix).GetLocalized().ToString();
				context.RegisterCodeFix(
					CodeAction.Create(
						title,
						c => Task.Run(() =>
							context.Document.WithSyntaxRoot(
								root.ReplaceNode(
									classNode.BaseList,
									BaseList(
										SeparatedList(
											new BaseTypeSyntax[]
											{
												SimpleBaseType(newBaseType)
											})))), c),
						title),
					context.Diagnostics);
			}
		}

		private TypeSyntax CreateBaseType(string typeName, string dacFieldName)
		{
			if (!LegacyBqlFieldFix.PropertyTypeToFieldType.ContainsKey(typeName))
				return null;

			return IdentifierName($"PX.Data.BQL.Bql{LegacyBqlFieldFix.PropertyTypeToFieldType[typeName]}.Constant<{dacFieldName}>");
		}
	}
}