﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;

namespace PX.Objects
{
	/// <summary>
	/// 
	/// </summary>
	[PXCacheName("With Empty Summary")]
	public class WithEmptySummary : IBqlTable
	{
	}

	/// <summary/>
	/// <remarks>
	/// Check that remark is not lost by the code fix.
	/// </remarks>
	[PXCacheName("With Empty One Liner Summary")]
	public class WithEmptyOneLinerSummary : IBqlTable
	{
	}
}
