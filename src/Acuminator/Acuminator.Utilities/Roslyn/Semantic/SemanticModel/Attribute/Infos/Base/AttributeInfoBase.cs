#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// General information about attributes of an entity.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class AttributeInfoBase
	{
		/// <summary>
		/// The attribute placement.
		/// </summary>
		public abstract AttributePlacement Placement { get; }

		/// <summary>
		/// Information describing the attribute application.
		/// </summary>
		public AttributeData AttributeData { get; }

		public INamedTypeSymbol? AttributeType => AttributeData.AttributeClass;

		public virtual string Name => AttributeType?.Name ?? ToString();

		/// <summary>
		/// The index number of the attribute in the attribute declaration.
		/// </summary>
		public int DeclarationOrder { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";

		protected AttributeInfoBase(AttributeData attributeData, int declarationOrder)
		{
			AttributeData    = attributeData.CheckIfNull();
			DeclarationOrder = declarationOrder;
		}
	}
}