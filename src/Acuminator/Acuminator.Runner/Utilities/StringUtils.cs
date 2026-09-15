using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Utilities
{
	internal static class StringUtils
	{
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Enquote(this string? source) =>
			source.IsNullOrEmpty()
				? string.Empty
				: $"\"{source}\"";
	}
}
