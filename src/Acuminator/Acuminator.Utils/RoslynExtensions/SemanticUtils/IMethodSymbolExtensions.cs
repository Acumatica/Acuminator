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

			var parent = methodSymbol.ContainingType;
			if (parent != null)
			{
				return parent.InstanceConstructors.Contains(methodSymbol);
			}

			return false;
		}
	}
}
