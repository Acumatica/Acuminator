#nullable enable

using System;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// The symbols set base class.
	/// </summary>
	public abstract class SymbolsSetBase
	{
		protected Compilation Compilation { get; }

		private protected SymbolsSetBase(Compilation compilation)
		{
			Compilation = compilation.CheckIfNull(nameof(compilation));
		}
	}
}
