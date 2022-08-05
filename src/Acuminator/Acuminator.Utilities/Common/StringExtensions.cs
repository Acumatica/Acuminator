#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.Common
{
	public static class StringExtensions
	{
		/// <summary>
		/// A string extension method that returns null if passed string <paramref name="str"/> is null, empty or contains only whitespaces.
		/// </summary>
		/// <param name="str">The string to act on.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string? NullIfWhiteSpace(this string? str) =>
			string.IsNullOrWhiteSpace(str)
				? null
				: str;

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrWhiteSpace([NotNullWhen(returnValue: false)] this string? str) => string.IsNullOrWhiteSpace(str);

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty([NotNullWhen(returnValue: false)] this string? str) => string.IsNullOrEmpty(str);

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty(this string source)
		{
			source.ThrowOnNull(nameof(source));

			return source.Length == 0;
		}

		/// <summary>
		/// A string extension method that converts the string to a pascal case (first letter to upper case).
		/// </summary>
		/// <param name="s">The string to act on.</param>
		/// <returns/>
		public static string? ToPascalCase(this string? s)
		{
			if (s.IsNullOrWhiteSpace() || char.IsUpper(s[0]))
				return s;

			return s.Length > 1 
				? char.ToUpperInvariant(s[0]).ToString() + s.Substring(1)
				: char.ToUpperInvariant(s[0]).ToString();
		}

		/// <summary>
		/// Joins strings in <paramref name="strings"/> with a <paramref name="separator"/>. 
		/// This extension method is just a shortcut for the call to <see cref="String.Join(string, IEnumerable{string})"/> which allows to use API in a fluent way. 
		/// </summary>
		/// <param name="strings">The strings to act on.</param>
		/// <param name="separator">The separator.</param>
		/// <returns>
		/// A joined string.
		/// </returns>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Join(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

		/// <summary>
		/// Compute the distance between two strings.
		/// </summary>
		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.CheckIfNull(nameof(s)).Length;
			int m = t.CheckIfNull(nameof(t)).Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
	}
}
