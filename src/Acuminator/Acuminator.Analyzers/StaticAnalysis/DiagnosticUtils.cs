#nullable enable

using System;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFlagSet(this Diagnostic diagnostic, string flagName)
		{
			diagnostic.ThrowOnNull(nameof(diagnostic));
			flagName.ThrowOnNullOrWhiteSpace(nameof(flagName));

			return diagnostic.Properties?.Count > 0 && diagnostic.Properties.TryGetValue(flagName, out string value) &&
				   bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase);
		}
	}
}
