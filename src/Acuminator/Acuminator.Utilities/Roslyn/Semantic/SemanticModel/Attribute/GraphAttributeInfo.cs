#nullable enable

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Graph or graph extension attribute info.
	/// </summary>
	public class GraphAttributeInfo : AttributeInfoBase
	{
		/// <summary>
		/// Indicates if the attribute configures the default navigation.
		/// </summary>
		public bool IsDefaultNavigation { get; }

		/// <summary>
		/// Indicates if the attribute configures access to the protected members of a base graph or base graph extension.
		/// </summary>
		public bool IsProtectedAccess { get; }

		public GraphAttributeInfo(PXContext pxContext, AttributeData attributeData, int declarationOrder) : base(attributeData, declarationOrder)
		{
			IsDefaultNavigation = AttributeType.IsDefaultNavigation(pxContext);
			IsProtectedAccess   = pxContext.AttributeTypes.PXProtectedAccessAttribute is INamedTypeSymbol protectedAccessAttribute && 
								  AttributeType.InheritsFromOrEquals(protectedAccessAttribute);
		}
	}
}