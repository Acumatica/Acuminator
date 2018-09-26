using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class CFINCategory : IBqlTable
	{
		#region CategoryID
		public abstract class categoryID : IBqlField { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Category ID")]
		public virtual int? CategoryID { get; set; }
		#endregion
	
	}
}
