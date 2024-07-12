using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Values that represent merge method for attributes declared on a cache attached event.
	/// </summary>
	public enum CacheAttachedAttributesMergeMethod : byte
	{
		/// <summary>
		/// Is used to add custom attributes to the existing ones.
		/// </summary>
		Append,

		/// <summary>
		/// Forces the system to use custom attributes instead of the existing ones. 
		/// This option is used by default if you do not specify the <tt>PXMergeAttributes</tt>
		/// attribute on the customized field.
		/// </summary>
		Replace,

		/// <summary>
		/// Makes the system apply the union of the custom and existing attributes to the customized field.
		/// </summary>
		Merge
	}
}
