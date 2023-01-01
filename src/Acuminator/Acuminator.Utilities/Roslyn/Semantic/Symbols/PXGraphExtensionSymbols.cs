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

		public IMethodSymbol? Configure { get; }

		internal PXGraphExtensionSymbols(Compilation compilation) : base(compilation, TypeFullNames.PXGraphExtension)
        {
			Type.ThrowOnNull(nameof(Type));

			Initialize = GetMethod(DelegateNames.Initialize);
			Configure = GetConfigureMethod();
		}

		private IMethodSymbol? GetConfigureMethod()
		{
			var pxScreenConfiguration = Compilation.GetTypeByMetadataName(TypeFullNames.Workflow.PXScreenConfiguration);

			if (pxScreenConfiguration == null)
				return null;

			var configureMethods = Type!.GetMembers(DelegateNames.Workflow.Configure);

			if (configureMethods.IsDefaultOrEmpty)
				return null;

			return configureMethods.OfType<IMethodSymbol>()
								   .FirstOrDefault(method => method.ReturnsVoid && method.IsVirtual && method.DeclaredAccessibility == Accessibility.Public &&
															 method.Parameters.Length == 1 && pxScreenConfiguration.Equals(method.Parameters[0].Type));
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