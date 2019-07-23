using System;


namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Helper used to classify Acumatica attributes.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	
	public enum BoundType
	{
		Unknown = 0,
		Unbound = 1,
		DbBound = 2,
		NotDefined = 3
	}
}