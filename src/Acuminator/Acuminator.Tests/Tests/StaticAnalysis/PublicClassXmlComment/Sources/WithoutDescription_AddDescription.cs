using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

namespace PX.Objects
{
	/// <summary>
	/// Without description
	/// </summary>
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
