#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// The symbols set for some type base class.
	/// </summary>
	public abstract class SymbolsSetForTypeBase : SymbolsSetBase
	{
		public INamedTypeSymbol? Type { get; }

		public bool IsDefined => Type != null;

		private protected SymbolsSetForTypeBase(Compilation compilation, string typeName) : base(compilation)
		{
			Type = compilation.GetTypeByMetadataName(typeName);
		}

		private protected SymbolsSetForTypeBase(Compilation compilation, INamedTypeSymbol? type) : base(compilation)
		{
			Type = type;
		}
	}
}
