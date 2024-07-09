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
	/// General attribute info.
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public abstract class AttributeInfoBase
	{
		/// <summary>
		/// Information describing the attribute application.
		/// </summary>
		public AttributeData AttributeData { get; }

		public INamedTypeSymbol? AttributeType => AttributeData.AttributeClass;

		public virtual string Name => AttributeType?.Name ?? ToString();

		/// <summary>
		/// The declaration order.
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