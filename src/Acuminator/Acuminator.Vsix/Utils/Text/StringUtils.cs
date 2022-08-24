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
		private const int MaxLength = 1500;
		private static readonly string TextOverflowSuffix = Environment.NewLine + "...";

		/// <summary>
		/// Trim excess text if the text length is greater than <paramref name="maxTextLength"/>.
		/// </summary>
		/// <param name="text">The text to act on.</param>
		/// <param name="maxTextLength">(Optional) The maximum length of the text. Pass <see langword="null"/> to use a default <see cref="MaxLength"/></param>
		/// <param name="overflowSuffix">(Optional) The overflow suffix. Pass <see langword="null"/> to use a default <see cref="TextOverflowSuffix"/>
		/// Pass <see cref="string.Empty"/> to not add any suffix.</param>
		/// <returns>
		/// A trimmed text with suffix <paramref name="overflowSuffix"/>.
		/// </returns>
		public static string? TrimExcess(this string? text, int? maxTextLength = null, string? overflowSuffix = null)
		{
			maxTextLength ??= MaxLength;

			if (maxTextLength <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxTextLength), maxTextLength, "Value must be greater than 0");

			if (text.IsNullOrEmpty() || text.Length <= maxTextLength)
				return text;

			string trimmedText = text.Substring(0, maxTextLength.Value);
			overflowSuffix ??= TextOverflowSuffix;
			
			if (overflowSuffix.Length > 0)
			{
				trimmedText += overflowSuffix;
			}
			
			return trimmedText;
		}
	}
}
