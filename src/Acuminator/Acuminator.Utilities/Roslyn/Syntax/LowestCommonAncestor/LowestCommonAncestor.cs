using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public static class LowestCommonAncestor
	{
		public static LCAResultForTwoStatements GetCommonAncestorForSyntaxStatements(StatementSyntax x, StatementSyntax y)
		{
			x.ThrowOnNull(nameof(x));
			y.ThrowOnNull(nameof(y));

			//Depth is average O(log n) operation, worst case is O(n) but it isn't the case for the syntax tree which is wide but not very deep.
			//For statements we could consider depth constrained by MethodDeclarationSyntax 
			int depthX = x.Depth<MethodDeclarationSyntax, StatementSyntax>();
			int depthY = y.Depth<MethodDeclarationSyntax, StatementSyntax>();

			StatementSyntax currentX = x, prevX = null;
			StatementSyntax currentY = y, prevY = null;

			while (depthX != depthY)                //First get nodes on the equal levels of depth
			{
				if (depthX > depthY)
				{
					prevX = currentX;
					currentX = currentX.Parent<StatementSyntax>();
					depthX--;
				}
				else
				{
					prevY = currentY;
					currentY = currentY.Parent<StatementSyntax>();
					depthY--;
				}
			}

			while (currentX != currentY)          //Then move up the branches until nodes coincide
			{
				prevX = currentX;
				prevY = currentY;
				currentX = currentX.Parent<StatementSyntax>();
				currentY = currentY.Parent<StatementSyntax>();
			}

			return new LCAResultForTwoStatements(currentX, prevX, prevY);
		}
	}
}
