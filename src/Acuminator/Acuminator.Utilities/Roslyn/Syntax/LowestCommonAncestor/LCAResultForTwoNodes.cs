using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	[StructLayout(LayoutKind.Auto)]
	public readonly struct LCAResultForTwoNodes : IEquatable<LCAResultForTwoNodes>
	{
		public SyntaxNode Ancestor { get; }

		public SyntaxNode AncestorChildX { get; }

		public SyntaxNode AncestorChildY { get; }

		public LCAResultForTwoNodes(SyntaxNode ancestor, SyntaxNode ancestorChildX, SyntaxNode ancestorChildY)
		{
			Ancestor = ancestor;
			AncestorChildX = ancestorChildX;
			AncestorChildY = ancestorChildY;
		}

		public override bool Equals(object obj) =>
			obj is LCAResultForTwoNodes other
				? Equals(other)
				: false;

		public bool Equals(LCAResultForTwoNodes other) =>
			Equals(Ancestor, other.Ancestor) && Equals(AncestorChildX, other.AncestorChildX) && Equals(AncestorChildY, other.AncestorChildY);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + (Ancestor?.GetHashCode() ?? 0);
				hash = 23 * hash + (AncestorChildX?.GetHashCode() ?? 0);
				hash = 23 * hash + (AncestorChildY?.GetHashCode() ?? 0);
			}

			return hash;
		}
	}
}
