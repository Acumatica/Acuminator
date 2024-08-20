using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// The symbols set for some type base class.
	/// </summary>
	public abstract class SymbolsSetForTypeBase : SymbolsSetBase
	{
		public virtual INamedTypeSymbol Type { get; }

		[MemberNotNullWhen(returnValue: true, nameof(Type))]
		public bool IsDefined => Type != null;

		private protected SymbolsSetForTypeBase(Compilation compilation, string typeName) : base(compilation)
		{
			Type = compilation.GetTypeByMetadataName(typeName);
		}

		private protected SymbolsSetForTypeBase(Compilation compilation, INamedTypeSymbol type) : base(compilation)
		{
			Type = type;
		}
	}
}
