using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

using Acuminator.Runner.Output.Json;
using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Output.Data
{
	[JsonConverter(typeof(LineConverter))]
	internal readonly struct Line : IEquatable<Line>, IComparable<Line>
	{
		public ImmutableArray<LineSpan> Spans { get; }

		public Line(LineSpan line) =>
			Spans = [line];

		public Line(LineSpan idPart1, LineSpan messagePart, LineSpan locationPart) =>
			Spans = [idPart1, messagePart, locationPart];

		public Line(LineSpan severity, LineSpan idPart1, LineSpan messagePart, LineSpan locationPart) =>
			Spans = [severity, idPart1, messagePart, locationPart];

		public Line(IEnumerable<string> spans) =>
			Spans = spans.CheckIfNull()
						 .Select(s => new LineSpan(s))
						 .ToImmutableArray();

		public Line(IEnumerable<LineSpan> spans) =>
			Spans = spans.CheckIfNull().ToImmutableArray();

		public override string ToString() => Spans.Length switch
		{
			0 => string.Empty,
			1 => Spans[0].ToString(),
			2 => $"{Spans[0].ToString()} {Spans[1].ToString()}",
			3 => $"{Spans[0].ToString()} {Spans[1].ToString()} {Spans[2].ToString()}",
			4 => $"{Spans[0].ToString()} {Spans[1].ToString()} {Spans[2].ToString()} {Spans[3].ToString()}",
			_ => string.Join(" ", Spans)
		};

		public override bool Equals(object obj) =>
			obj is Line other && Equals(other);

		public bool Equals(Line other)
		{
			switch (Spans.Length)
			{
				case 0:
					return other.Spans.Length == 0;
				case 1 
				when other.Spans.Length == 1:
					return string.Equals(Spans[0].Text, other.Spans[0].Text, StringComparison.Ordinal);

				default:
					string line		 = ToString();
					string otherLine = other.ToString();

					return string.Equals(line, otherLine, StringComparison.Ordinal);
			}
		}

		public int CompareTo(Line other)
		{
			switch (Spans.Length)
			{
				case 0:
					return other.Spans.Length == 0
						? 0
						: -1;
				case 1
				when other.Spans.Length == 1:
					return Spans[0].CompareTo(other.Spans[0]);

				default:
					if (other.Spans.Length == 0)
						return 1;

					string line		 = ToString();
					string otherLine = other.ToString();

					return string.Compare(line, otherLine, StringComparison.Ordinal);
			}
		}

		public override int GetHashCode()
		{
			if (Spans.Length == 1)
				return Spans[0].GetHashCode();
			else if (Spans.Length > 1)
			{
				int hash = 17;

				unchecked
				{
					foreach (var span in Spans)
						hash = 23 * hash + span.GetHashCode();
				}

				return hash;
			}
			else
				return 0;
		}
	}
}
