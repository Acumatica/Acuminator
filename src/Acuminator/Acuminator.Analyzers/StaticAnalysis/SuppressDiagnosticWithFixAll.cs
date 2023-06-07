#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Acuminator.Analyzers.StaticAnalysis
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class SuppressDiagnosticWithFixAll : SuppressDiagnosticFixBase
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; }

		public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public SuppressDiagnosticWithFixAll()
		{
			FixableDiagnosticIds = AllCollectedFixableDiagnosticIds.Where(id => DiagnosticIdsWithFixAllEnabled.Contains(id))
																   .ToImmutableArray();
		}
	}
}
