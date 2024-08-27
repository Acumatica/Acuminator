
using System;

namespace Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
	/// <summary>
	/// Kinds of symbols checked by PX1022 diagnostic.
	/// </summary>
	internal enum CheckedSymbolKind
	{
		Dac,
		Graph,
		DacExtension,
		GraphExtension
	}
}