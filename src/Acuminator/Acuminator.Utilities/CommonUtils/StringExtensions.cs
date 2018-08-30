using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace Acuminator.Utilities
{
	public static class StringExtensions
	{
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

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
		public static string ToPascalCase(this string s)
		{
			if (s.IsNullOrWhiteSpace() || char.IsUpper(s[0]))
				return s;

			return s.Length > 1 
				? char.ToUpperInvariant(s[0]).ToString() + s.Substring(1)
				: char.ToUpperInvariant(s[0]).ToString();
		}

		/// <summary>
		/// Compute the distance between two strings.
		/// </summary>
		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
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
