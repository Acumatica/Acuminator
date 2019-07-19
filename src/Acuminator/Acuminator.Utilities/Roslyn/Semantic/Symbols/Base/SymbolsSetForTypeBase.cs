using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// The symbols set for some type base class.
	/// </summary>
	public abstract class SymbolsSetForTypeBase : SymbolsSetBase
	{
		public INamedTypeSymbol Type { get; }

		public bool IsDefined => Type != null;

		internal SymbolsSetForTypeBase(Compilation compilation, string typeName) : base(compilation)
		{
			Type = compilation.GetTypeByMetadataName(typeName);
		}
	}
}
