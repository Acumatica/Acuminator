using System;
using System.Diagnostics.CodeAnalysis;

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
