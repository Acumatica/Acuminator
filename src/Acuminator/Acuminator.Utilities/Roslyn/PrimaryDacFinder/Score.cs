#nullable enable

using System;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder
{
	public class Score : IEquatable<Score>, IComparable<Score>
	{
		public double Value
		{
			get;
			set;
		}

		public Score(double value)
		{
			Value = value;
		}

		public int CompareTo(Score? other) =>
			other != null
				? Value.CompareTo(other.Value)
				: 1;

		public override bool Equals(object obj) => Equals(obj as Score);

		public bool Equals(Score? other) => ReferenceEquals(this, other) || Value == other?.Value;

		public override int GetHashCode() => Value.GetHashCode();

		public override string ToString() => Value.ToString();
	}
}
