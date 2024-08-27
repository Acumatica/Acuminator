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
			diagnostic.ThrowOnNull();

			return TryGetPropertyValueInternal(diagnostic, DiagnosticProperty.RegisterCodeFix, out string? registered) 
				? bool.TrueString.Equals(registered, StringComparison.OrdinalIgnoreCase)
				: considerRegisteredByDefault;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsFlagSet(this Diagnostic diagnostic, string flagName, bool? defaultValueForMissingFlag = null)
		{
			diagnostic.ThrowOnNull();
			flagName.ThrowOnNullOrWhiteSpace();

			bool defaultValue = defaultValueForMissingFlag ?? false;

			if (!TryGetPropertyValueInternal(diagnostic, flagName, out string? flagValueStr))
				return defaultValue;
			else
				return bool.TrueString.Equals(flagValueStr, StringComparison.OrdinalIgnoreCase);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryGetPropertyValue(this Diagnostic diagnostic, string propertyName, out string? propertyValue) =>
			TryGetPropertyValueInternal(diagnostic.CheckIfNull(), 
										propertyName.CheckIfNullOrWhiteSpace(), 
										out propertyValue);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool TryGetPropertyValueInternal(Diagnostic diagnostic, string propertyName, out string? propertyValue)
		{
			if (diagnostic.Properties?.Count > 0 && diagnostic.Properties.TryGetValue(propertyName, out propertyValue))
				return true;
			else
			{
				propertyValue = null;
				return false;
			}
		}
	}
}
