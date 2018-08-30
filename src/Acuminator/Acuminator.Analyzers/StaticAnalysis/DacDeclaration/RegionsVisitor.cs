using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.DacDeclaration
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
