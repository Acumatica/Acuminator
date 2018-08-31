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

		public int CompareTo(Score other)
		{
			if (other == null)
				return 1;
			else if (Value == other.Value)
				return 0;
			else if (Value > other.Value)
				return 1;
			else
				return -1;
		}

		public bool Equals(Score other)
		{
			if (other == null)
				return false;
			else if (ReferenceEquals(this, other))
				return true;
			else
				return Value == other.Value;
		}

		public override bool Equals(object obj) => Equals(obj as Score);
		
		public override int GetHashCode() => Value.GetHashCode();

		public override string ToString() => Value.ToString();	
	}
}
