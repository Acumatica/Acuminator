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
		protected readonly Compilation _compilation;

		internal SymbolsSetBase(Compilation compilation)
		{
			_compilation = compilation;
		}
	}
}
