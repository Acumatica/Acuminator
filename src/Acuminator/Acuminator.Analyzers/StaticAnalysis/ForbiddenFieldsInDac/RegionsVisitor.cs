using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac
{
	public partial class ForbiddenFieldsInDacFix : CodeFixProvider
	{	
		/// <summary>
		/// The #regions rewriter helper.
		/// </summary>
		private class RegionsVisitor : CSharpSyntaxWalker
		{
			private readonly Stack<RegionDirectiveTriviaSyntax> _regionsStack;
			private readonly string _identifierToRemove;
			private readonly CancellationToken _cancellationToken;

			public List<DirectiveTriviaSyntax> RegionNodesToRemove { get; } = new List<DirectiveTriviaSyntax>(capacity: 2);

			public RegionsVisitor(string identifierToDelete, CancellationToken cToken) : base(SyntaxWalkerDepth.StructuredTrivia)
			{
				_regionsStack = new Stack<RegionDirectiveTriviaSyntax>(capacity: 4);
				_identifierToRemove = identifierToDelete.ToUpperInvariant();
				_cancellationToken = cToken;
			}

			public override void VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax regionDirective)
			{
				if (_cancellationToken.IsCancellationRequested)
					return;

				_regionsStack.Push(regionDirective);
				string regionNameUpperCase = GetRegionName(regionDirective).ToUpperInvariant();

				if (regionNameUpperCase.Contains(_identifierToRemove))
				{
					RegionNodesToRemove.Add(regionDirective);
				}

				base.VisitRegionDirectiveTrivia(regionDirective);
			}

			public override void VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax endRegionDirective)
			{
				if (_regionsStack.Count == 0 || _cancellationToken.IsCancellationRequested)
				{
					if (!_cancellationToken.IsCancellationRequested)
						base.VisitEndRegionDirectiveTrivia(endRegionDirective);

					return;
				}

				RegionDirectiveTriviaSyntax regionDirective = _regionsStack.Pop();
				string regionNameUpperCase = GetRegionName(regionDirective).ToUpperInvariant();

				if (regionNameUpperCase.Contains(_identifierToRemove))
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
