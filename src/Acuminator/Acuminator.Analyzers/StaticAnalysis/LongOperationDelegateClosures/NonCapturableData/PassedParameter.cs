#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	/// <summary>
	/// Values that represent captured instances types.
	/// </summary>
	[Flags]
	internal enum CapturedInstancesTypes
	{
		None = 0,

		/// <summary>
		/// Captured instance is PXGraph.
		/// </summary>
		PXGraph = 0b01,

		/// <summary>
		/// Captured instance is PXAdapter.
		/// </summary>
		PXAdapter = 0b10
	}

	/// <summary>
	/// Information about the passed parameter
	/// </summary>
	internal readonly struct PassedParameter : IEquatable<PassedParameter>
	{
		public string Name { get; }

		public CapturedInstancesTypes CapturedTypes { get; }

		public bool CaptureGraph => (CapturedTypes & CapturedInstancesTypes.PXGraph) == CapturedInstancesTypes.PXGraph;

		public bool CaptureAdapter => (CapturedTypes & CapturedInstancesTypes.PXAdapter) == CapturedInstancesTypes.PXAdapter;

		public PassedParameter(string name, CapturedInstancesTypes capturedInstanceTypes)
		{
			Name = name.CheckIfNullOrWhiteSpace(nameof(name));
			CapturedTypes = capturedInstanceTypes;
		}

		public override bool Equals(object obj) => obj is PassedParameter other && Equals(other);

		public bool Equals(PassedParameter other) =>
			Name == other.Name && CapturedTypes == other.CapturedTypes;

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Name.GetHashCode();
				hash = 23 * hash + (int)CapturedTypes;
			}

			return hash;
		}
	}
}