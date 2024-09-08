#nullable enable

namespace Acuminator.Utilities.BannedApi.Model
{
	/// <summary>
	/// Values that represent for whom API is banned.
	/// </summary>
	public enum ApiBanKind : byte
	{
		/// <summary>
		/// Api is banned for all.
		/// </summary>
		General,

		/// <summary>
		/// Api is banned for ISVs.
		/// </summary>
		ISV
	}

	public static class ApiBanKindUtils
	{
		public static ApiBanKind Combine(this ApiBanKind banKindX, ApiBanKind banKindY) =>
			banKindX == banKindY
				? banKindX
				: ApiBanKind.General;
	}
}