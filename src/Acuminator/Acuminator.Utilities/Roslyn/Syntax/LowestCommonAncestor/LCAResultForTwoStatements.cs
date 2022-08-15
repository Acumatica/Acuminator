using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	[StructLayout(LayoutKind.Auto)]
	public readonly struct LCAResultForTwoStatements : IEquatable<LCAResultForTwoStatements>
	{
		public StatementSyntax Ancestor { get; }

		public StatementSyntax ScopedX { get; }

		public StatementSyntax ScopedY { get; }

		public LCAResultForTwoStatements(StatementSyntax ancestor, StatementSyntax scopedX, StatementSyntax scopedY)
		{
			Ancestor = ancestor;
			ScopedX = scopedX;
			ScopedY = scopedY;
		}

		public override bool Equals(object obj) =>
			obj is LCAResultForTwoStatements other
				? Equals(other)
				: false;

		public bool Equals(LCAResultForTwoStatements other) =>
			Equals(Ancestor, other.Ancestor) && Equals(ScopedX, other.ScopedX) && Equals(ScopedY, other.ScopedY);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + (Ancestor?.GetHashCode() ?? 0);
				hash = 23 * hash + (ScopedX?.GetHashCode() ?? 0);
				hash = 23 * hash + (ScopedY?.GetHashCode() ?? 0);
			}

			return hash;
		}
	}
}
