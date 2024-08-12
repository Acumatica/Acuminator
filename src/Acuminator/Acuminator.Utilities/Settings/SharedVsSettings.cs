#nullable enable

namespace Acuminator.Utilities
{
	public static class SharedVsSettings
	{
		public const string AcuminatorSharedMemorySlotName = "AcuminatorMemorySlot";

		public static VSVersion? VSVersion { get; set; }

		public static bool IsInsideVsProcess => VSVersion != null; 
	}
}
