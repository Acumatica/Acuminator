using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionMessageComparer : IComparer<SuppressMessage>
	{
		public int Compare(SuppressMessage msgX, SuppressMessage msgY) =>
			string.CompareOrdinal(msgX.Id, msgY.Id) is var idComparison && idComparison != 0
				? idComparison
				: string.CompareOrdinal(msgX.Target, msgY.Target) is var targetComparison && targetComparison != 0
					? targetComparison
					: string.CompareOrdinal(msgX.SyntaxNode, msgY.SyntaxNode);
	}
}
