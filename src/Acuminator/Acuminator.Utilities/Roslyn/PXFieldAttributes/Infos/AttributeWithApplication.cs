#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Information about the attribute with attribute application.
	/// </summary>
	public class AttributeWithApplication : IEquatable<AttributeWithApplication>
	{
		public ITypeSymbol Type { get; }

		public AttributeData Application { get; }

		public AttributeWithApplication(AttributeData attributeApplication) : this(attributeApplication, attributeApplication?.AttributeClass!)
		{
		}

		public AttributeWithApplication(AttributeData attributeApplication, ITypeSymbol attributeType)
		{
			Application = attributeApplication.CheckIfNull();
			Type = attributeType.CheckIfNull();
		}

		public override bool Equals(object obj) => obj is AttributeWithApplication other && Equals(other);

		public bool Equals(AttributeWithApplication other) =>
			Type == other.Type && Application.Equals(other.Application);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = 23 * hash + Type.GetHashCode();
				hash = 23 * hash + Application.GetHashCode();
			}

			return hash;
		}

		public void Deconstruct(out ITypeSymbol attributeType, out AttributeData attributeApplication)
		{
			attributeType = Type;
			attributeApplication = Application;
		}

		public override string ToString() => $"{Type}: {Application}";
	}
}
