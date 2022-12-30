#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
    public class PXGraphExtensionSymbols : SymbolsSetForTypeBase
	{
		public IMethodSymbol? Initialize { get; }

		internal PXGraphExtensionSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXGraphExtension)
        {
			Type.ThrowOnNull(nameof(Type));

			Initialize = GetMethod(DelegateNames.Initialize);
        }

		private IMethodSymbol? GetMethod(string methodName)
		{
			var methods = Type!.GetMembers(methodName);
			return methods.IsDefaultOrEmpty
				? null
				: methods.OfType<IMethodSymbol>()
						 .FirstOrDefault();
		}
    }
}
