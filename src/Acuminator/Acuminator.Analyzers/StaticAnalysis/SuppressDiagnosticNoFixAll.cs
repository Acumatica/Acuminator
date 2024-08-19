
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
	public class SuppressDiagnosticNoFixAll : SuppressDiagnosticFixBase
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; }

		public SuppressDiagnosticNoFixAll()
		{
			FixableDiagnosticIds = AllCollectedFixableDiagnosticIds.Where(id => !DiagnosticIdsWithFixAllEnabled.Contains(id))
																   .ToImmutableArray();
		}

		/// <summary>
		/// Explicitly disable Fix All for diagnostics.
		/// </summary>
		/// <returns/>
		public override FixAllProvider? GetFixAllProvider() => null;
	}
}
