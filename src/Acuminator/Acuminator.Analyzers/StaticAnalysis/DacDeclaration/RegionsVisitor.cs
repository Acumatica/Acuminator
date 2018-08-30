using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CSharp.Formatting;

namespace Acuminator.Analyzers.FixProviders
{
	public partial class ForbiddenFieldsInDacFix : CodeFixProvider
	{	
		/// <summary>
		/// The #regions rewriter helper.
		/// </summary>
		private class RegionsVisitor : CSharpSyntaxWalker
		{
			private readonly Stack<RegionDirectiveTriviaSyntax> regionsStack;
			private readonly string identifierToRemove;
			private readonly CancellationToken cancellationToken;

			public List<DirectiveTriviaSyntax> RegionNodesToRemove { get; } = new List<DirectiveTriviaSyntax>(capacity: 2);

			public RegionsVisitor(string identifierToDelete, CancellationToken cToken) : base(SyntaxWalkerDepth.StructuredTrivia)
			{
				regionsStack = new Stack<RegionDirectiveTriviaSyntax>(capacity: 4);
				identifierToRemove = identifierToDelete.ToUpperInvariant();
				cancellationToken = cToken;
			}

			public override void VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax regionDirective)
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				regionsStack.Push(regionDirective);
				string regionNameUpperCase = GetRegionName(regionDirective).ToUpperInvariant();

				if (regionNameUpperCase.Contains(identifierToRemove))
				{
					RegionNodesToRemove.Add(regionDirective);
				}

				base.VisitRegionDirectiveTrivia(regionDirective);
			}

			public override void VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax endRegionDirective)
			{
				if (regionsStack.Count == 0 || cancellationToken.IsCancellationRequested)
				{
					if (!cancellationToken.IsCancellationRequested)
						base.VisitEndRegionDirectiveTrivia(endRegionDirective);

					return;
				}

				RegionDirectiveTriviaSyntax regionDirective = regionsStack.Pop();
				string regionNameUpperCase = GetRegionName(regionDirective).ToUpperInvariant();

				if (regionNameUpperCase.Contains(identifierToRemove))
				{
					RegionNodesToRemove.Add(endRegionDirective);
				}

				base.VisitEndRegionDirectiveTrivia(endRegionDirective);
			}

			private static string GetRegionName(RegionDirectiveTriviaSyntax regionDirective) =>
				regionDirective.EndOfDirectiveToken.LeadingTrivia.ToString();
		}
	}
}
