using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace PX.Analyzers.Vsix.Formatter
{
	internal static class BqlFormatter
	{
		public static SyntaxNode Format(SyntaxNode syntaxRoot, SemanticModel semanticModel)
		{
			return syntaxRoot;
		}
	}
}
