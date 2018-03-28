using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Vsix.Formatter
{
	public static class SyntaxTriviaListExtensions
	{
		public static int LastIndexOf(this SyntaxTriviaList list, SyntaxKind kind)
		{
			int rawKind = (int) kind;
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].RawKind == rawKind) return i;
			}

			return -1;
		}
	}
}
