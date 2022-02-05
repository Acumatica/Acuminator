#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Utilities
{
	public static class StringUtils
	{
		/// <summary>
		/// The maximum length of the text. Usually used to restrict length of tooltips
		/// </summary>
		private const int MaxLength = 1000;
		private const string TextOverflowSuffix = "...";

		/// <summary>
		/// Trim excess text if the text length is greater than <see cref="MaxLength"/>.
		/// </summary>
		/// <param name="text">The text to act on.</param>
		/// <returns>
		/// A trimmed text with <see cref="TextOverflowSuffix"/>
		/// </returns>
		public static string? TrimExcess(this string? text)
		{
			if (text.IsNullOrEmpty() || text!.Length <= MaxLength)
				return text;

			string trimmedText = text.Substring(0, MaxLength) + TextOverflowSuffix;
			return trimmedText;
		}
	}
}
