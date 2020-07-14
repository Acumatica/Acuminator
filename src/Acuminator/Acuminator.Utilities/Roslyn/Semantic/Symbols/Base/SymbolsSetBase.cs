using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// The symbols set base class.
	/// </summary>
	public abstract class SymbolsSetBase
	{
		protected Compilation Compilation { get; }

		internal SymbolsSetBase(Compilation compilation)
		{
			Compilation = compilation;
		}
	}
}
