#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Information about parameter passed into method 
	/// </summary>
	internal readonly struct PassedParameter : IEquatable<PassedParameter>
	{
		public PassedParameterKind Kind { get; }

		public string Name { get; }

		public PassedParameter(PassedParameterKind kind, string name)
		{
			Kind = kind;
			Name = name.CheckIfNullOrWhiteSpace(nameof(name));
		}

		public override string ToString() => $"Name: {Name}, Kind: {Kind}";

		public override bool Equals(object obj) => obj is PassedParameter other && Equals(other);

		public bool Equals(PassedParameter other) => Kind == other.Kind && Name == other.Name;

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Kind.GetHashCode();
				hash = 23 * hash + Name.GetHashCode();
			}

			return hash;
		}
	}
}