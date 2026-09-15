using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output.Data
{
	internal readonly struct LineSpan : IEquatable<LineSpan>, IComparable<LineSpan>
	{
		public string? Text { get; }

		public ConsoleColor? ForegroundColor { get; }

        public LineSpan(string text, ConsoleColor? foregroundColor = null)
        {
			Text = text.CheckIfNullOrWhiteSpace();
			ForegroundColor = foregroundColor;
        }

		public override bool Equals(object obj) =>
			obj is LineSpan other && Equals(other);

		public bool Equals(LineSpan other)
		{
			if (ForegroundColor != other.ForegroundColor)
				return false;

			if (Text == null)
				return other.Text == null;
			else
				return Text.Equals(other.Text, StringComparison.Ordinal);
		}

		public int CompareTo(LineSpan other)
		{
			if (Text == null && other.Text == null)
				return 0;
			else if (other.Text == null)
				return 1;
			else if (Text == null)
				return -1;
			else
				return string.Compare(Text, other.Text, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + (Text?.GetHashCode() ?? 0);
				hash = 23 * hash + ForegroundColor.GetHashCode();
			}

			return hash;
		}

		public override string ToString() => Text ?? string.Empty;
	}
}
