using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	[StructLayout(LayoutKind.Auto)]
	public readonly struct EventInfo : IEquatable<EventInfo>
	{
		public EventType Type { get; }

		public EventHandlerSignatureType SignatureType { get; }

		public EventInfo(EventType type, EventHandlerSignatureType signatureType)
		{
			Type = type;
			SignatureType = signatureType;
		}

		public static EventInfo None() => new EventInfo(EventType.None, EventHandlerSignatureType.None);

		public override bool Equals(object obj) =>
			obj is EventInfo other
				? Equals(other)
				: false;

		public bool Equals(EventInfo other) => Type == other.Type && SignatureType == other.SignatureType;

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Type.GetHashCode();
				hash = 23 * hash + SignatureType.GetHashCode();
			}

			return hash;
		}

		public override string ToString() => $"Type: {Type.ToString()}, Signature Type: {SignatureType.ToString()}";
	}
}
