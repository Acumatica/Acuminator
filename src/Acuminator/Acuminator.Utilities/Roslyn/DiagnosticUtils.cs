using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;


namespace Acuminator.Utilities.Roslyn
{
	/// <summary>
	/// A helper class with extension methods for Roslyn <see cref="Diagnostic"/> objects.
	/// </summary>
	public static class DiagnosticUtils
	{
		public static bool IsAcuminatorDiagnostic(this Diagnostic diagnostic)
		{
			diagnostic.ThrowOnNull(nameof(diagnostic));
			return diagnostic.Id.StartsWith(SharedConstants.AcuminatorDiagnosticPrefix);
		}
	}
}
