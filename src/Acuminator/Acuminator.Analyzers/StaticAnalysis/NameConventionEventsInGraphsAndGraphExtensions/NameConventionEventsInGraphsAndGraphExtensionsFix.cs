#nullable enable

using System;
using System.Collections;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class NameConventionEventsInGraphsAndGraphExtensionsFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
											 .ConfigureAwait(false);

			var method = root?.FindNode(context.Span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (method == null || context.CancellationToken.IsCancellationRequested)
				return;

			string codeActionName = nameof(Resources.PX1000Fix).GetLocalized().ToString();
			context.RegisterCodeFix(
				new ChangeSignatureAction(codeActionName, context.Document, method),
				context.Diagnostics);
		}
	}
}