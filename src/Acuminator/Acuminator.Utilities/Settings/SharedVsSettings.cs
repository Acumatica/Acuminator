using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Acuminator.Utilities
{
	public static class SharedVsSettings
	{
		public const string AcuminatorSharedMemorySlotName = "AcuminatorMemorySlot";

		public static VSVersion? VSVersion { get; set; }

		[MemberNotNullWhen(returnValue: true, nameof(VSVersion))]
		public static bool IsInsideVsProcess => VSVersion != null; 
	}
}
