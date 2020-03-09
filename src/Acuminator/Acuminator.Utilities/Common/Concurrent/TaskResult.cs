using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Acuminator.Utilities.Common
{
	[StructLayout(LayoutKind.Auto)]
	public readonly struct TaskResult<TResult>
	{
		public bool IsSuccess { get; }
					
		public TResult Result { get; }

		public TaskResult(bool isSuccess, TResult result)
		{
			IsSuccess = isSuccess;
			Result = result;
		}
	}
}
