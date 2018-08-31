using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis
{
	internal static class DiagnosticUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsRegisteredForCodeFix(this Diagnostic diagnostic, bool considerRegisteredByDefault = true)
		{
			diagnostic.ThrowOnNull(nameof(diagnostic));

			return diagnostic.Properties.TryGetValue(DiagnosticProperty.RegisterCodeFix, out string registered) 
				? registered == bool.TrueString
				: considerRegisteredByDefault;
		}
	}
}
