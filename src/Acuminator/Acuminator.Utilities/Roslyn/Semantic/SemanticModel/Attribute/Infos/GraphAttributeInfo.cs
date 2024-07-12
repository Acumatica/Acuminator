#nullable enable

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about an attribute of a graph or a graph extension.
	/// </summary>
	public class GraphAttributeInfo : AttributeInfoBase
	{
		public override AttributePlacement Placement => AttributePlacement.Graph;

		/// <summary>
		/// An indicator of whether the attribute configures the default navigation.
		/// </summary>
		public bool IsDefaultNavigation { get; }

		/// <summary>
		/// An indicator of whether the attribute configures access to the protected members of a base graph or base graph extension.
		/// </summary>
		public bool IsProtectedAccess { get; }

		public GraphAttributeInfo(PXContext pxContext, AttributeData attributeData, int declarationOrder) : base(attributeData, declarationOrder)
		{
			if (AttributeType != null)
			{
				IsDefaultNavigation = AttributeType.IsDefaultNavigation(pxContext);
				IsProtectedAccess   = pxContext.AttributeTypes.PXProtectedAccessAttribute is { } protectedAccessAttribute &&
									  AttributeType.InheritsFromOrEquals(protectedAccessAttribute);
			}
		}
	}
}