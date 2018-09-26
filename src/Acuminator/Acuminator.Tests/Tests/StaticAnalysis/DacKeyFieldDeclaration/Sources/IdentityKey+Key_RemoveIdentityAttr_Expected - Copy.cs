using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class CFINCategory : IBqlTable
	{
		#region CategoryID
		public abstract class categoryID : IBqlField { }
		[PXUIField(DisplayName = "Category ID")]
		public virtual int? CategoryID { get; set; }
		#endregion
		#region CategoryCD
		public abstract class categoryCD : IBqlField { }
		[PXDBString(10, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Category ID")]
		public virtual string CategoryCD { get; set; }
		#endregion

		#region CompositeID
		public abstract class compositeID : IBqlField { }

		[PXInt]
		[PXUIField(DisplayName = "CompositeID")]
		public virtual int? CompositeID { get; set; }
		#endregion
	}
}
