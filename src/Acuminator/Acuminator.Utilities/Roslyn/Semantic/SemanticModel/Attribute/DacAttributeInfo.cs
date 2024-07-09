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
		public bool IsProjectionAttribute { get; }

		public DacAttributeInfo(PXContext pxContext, AttributeData attributeData, int declarationOrder) : base(attributeData, declarationOrder)
		{
			IsDefaultNavigation   = AttributeType.IsDefaultNavigation(pxContext);
			IsProjectionAttribute = AttributeType.InheritsFromOrEquals(pxContext.AttributeTypes.PXProjectionAttribute);
		}
	}
}