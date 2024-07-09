#nullable enable

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// DAC or DAC extension attribute info.
	/// </summary>
	public class DacAttributeInfo : AttributeInfoBase
	{
		/// <summary>
		/// Indicates if the attribute configures the default navigation.
		/// </summary>
		public bool IsDefaultNavigation { get; }

		/// <summary>
		/// Indicates if the attribute configures a projection DAC.
		/// </summary>
		public bool IsPXProjection { get; }

		/// <summary>
		/// Indicates if the attribute is a PXCacheNameAttribute.
		/// </summary>
		public bool IsPXCacheName { get; }

		/// <summary>
		/// Indicates if the attribute is a PXHiddenAttribute.
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