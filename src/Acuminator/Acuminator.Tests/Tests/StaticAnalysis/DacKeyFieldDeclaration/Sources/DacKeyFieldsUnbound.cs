using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class CFINCategory : PX.Data.IBqlTable
	{
		#region CategoryID
		public abstract class categoryID : PX.Data.IBqlField
		{
		}
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Category ID")]
		public virtual int? CategoryID { get; set; }
		#endregion
		#region CategoryCD
		public abstract class categoryCD : PX.Data.IBqlField
		{
		}
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Category ID")]
		public virtual string CategoryCD { get; set; }
		#endregion
	}
}
