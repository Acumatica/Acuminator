using System;
using System.Collections.Generic;
using System.Linq;



namespace Acuminator.Analyzers
{
	/// <summary>
	/// A class with string constatns representing diagnostic property names. They are used to pass custom data strings from diagnostic to code fix.
	/// </summary>
	internal static class DiagnosticProperty
	{
		public const string RegisterCodeFix = "RegisterCodeFix";
	}
}
