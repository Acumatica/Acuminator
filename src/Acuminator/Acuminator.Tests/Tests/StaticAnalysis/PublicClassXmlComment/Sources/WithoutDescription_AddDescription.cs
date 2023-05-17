using System;

using PX.Data;

namespace PX.Objects
{
	/// <summary>
	/// Without description
	/// </summary>
	[PXCacheName("Without description")]
	public class WithoutDescription : IBqlTable
	{
	}

	/// <summary>
	/// Without description but with remark
	/// </summary>
	/// <remarks>
	/// Test remark is not lost
	/// </remarks>
	[PXCacheName("Without description but with remark")]
	public class WithoutDescriptionButWithRemark : IBqlTable
	{
	}
}
