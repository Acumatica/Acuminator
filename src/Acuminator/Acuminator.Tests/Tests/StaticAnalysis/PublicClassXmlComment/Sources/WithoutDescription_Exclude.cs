using System;

using PX.Data;

namespace PX.Objects
{
	/// <exclude/>
	[PXCacheName("Without description")]
	public class WithoutDescription : IBqlTable
	{
	}

	/// <exclude/>
	/// <remarks>
	/// Test remark is not lost
	/// </remarks>
	[PXCacheName("Without description but with remark")]
	public class WithoutDescriptionButWithRemark : IBqlTable
	{
	}
}
