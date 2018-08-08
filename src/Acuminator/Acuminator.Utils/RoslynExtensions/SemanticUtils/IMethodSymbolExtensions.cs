using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities
{
	public static class IMethodSymbolExtensions
	{
		public static bool IsInstanceConstructor(this IMethodSymbol methodSymbol)
		{
			if (methodSymbol == null) throw new ArgumentNullException(nameof (methodSymbol));

			return !methodSymbol.IsStatic && methodSymbol.MethodKind == MethodKind.Constructor;
		}
	}
}
