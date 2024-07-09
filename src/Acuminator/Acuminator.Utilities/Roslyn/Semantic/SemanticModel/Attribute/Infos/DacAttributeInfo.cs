#nullable enable

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about an attribute of a DAC or a DAC extension.
	/// </summary>
	public class DacAttributeInfo : AttributeInfoBase
	{
		public override AttributePlacement Placement => AttributePlacement.Dac;

		/// <summary>
		/// An indicator of whether the attribute configures the default navigation.
		/// </summary>
		public bool IsDefaultNavigation { get; }

		/// <summary>
		/// An indicator of whether the attribute configures a projection DAC.
		/// </summary>
		public bool IsPXProjection { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXCacheName attribute.
		/// </summary>
		public bool IsPXCacheName { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXHidden attribute.
		/// </summary>
		public bool IsPXHidden { get; }

		public DacAttributeInfo(PXContext pxContext, AttributeData attributeData, int declarationOrder) : base(attributeData, declarationOrder)
		{
			if (AttributeType != null)
			{
				IsDefaultNavigation = AttributeType.IsDefaultNavigation(pxContext);
				IsPXProjection 		= AttributeType.InheritsFromOrEquals(pxContext.AttributeTypes.PXProjectionAttribute);
				IsPXCacheName 		= AttributeType.InheritsFromOrEquals(pxContext.AttributeTypes.PXCacheNameAttribute);
				IsPXHidden 			= AttributeType.InheritsFromOrEquals(pxContext.AttributeTypes.PXHiddenAttribute);
			}
		}
	}
}