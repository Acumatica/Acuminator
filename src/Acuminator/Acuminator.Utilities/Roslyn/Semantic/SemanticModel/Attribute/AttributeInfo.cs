using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	///  A  class for attributes
	/// </summary>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class AttributeInfo
	{
		/// <summary>
		/// Information describing the attribute.
		/// </summary>
		public AttributeData AttributeData { get; }

		public INamedTypeSymbol AttributeType => AttributeData.AttributeClass;

		public virtual string Name => AttributeType.Name;

		/// <summary>
		/// The declaration order.
		/// </summary>
		public int DeclarationOrder { get; }

		public BoundType BoundType { get; }

		public AttributeInfo(AttributeData attributeData, BoundType boundType, int declarationOrder)
		{
			attributeData.ThrowOnNull(nameof(attributeData));
			AttributeData = attributeData;
			BoundType = boundType;
			DeclarationOrder = declarationOrder;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected virtual string DebuggerDisplay => $"{Name}";
	}
}
