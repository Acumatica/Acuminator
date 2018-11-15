using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.GraphDeclarationTypeParameter
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class GraphDeclarationTypeParameterFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1093_GraphDeclarationVialation.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
		}
	}
}
