using PX.Data;

namespace PX.Objects
{
	[PXCacheName("Non public DAC")]
	internal class NonPublicDac : IBqlTable
	{
	}

	/// <summary>
	/// A public DAC with non public prroperties.
	/// </summary>
	[PXCacheName("A public DAC with non public prroperties")]
	public class PublicDacWithNonPublicPrroperties : IBqlTable
	{
		#region OrderType
		internal abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		internal string OrderType { get; set; }
		#endregion
	}
}
