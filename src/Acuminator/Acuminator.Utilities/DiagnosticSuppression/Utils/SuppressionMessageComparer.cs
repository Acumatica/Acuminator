using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionMessageComparer : IComparer<SuppressMessage>
	{
		public int Compare(SuppressMessage msgX, SuppressMessage msgY)
		{
			int idComparison = string.CompareOrdinal(msgX.Id, msgY.Id);

			if (idComparison != 0)
				return idComparison;

			int targetComparison = string.CompareOrdinal(msgX.Target, msgY.Target);

			if (targetComparison != 0)
				return targetComparison;

			return string.CompareOrdinal(msgX.SyntaxNode, msgY.SyntaxNode);
		}
	}
}
