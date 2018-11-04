using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacKeyFieldDeclaration.Sources
{
	public class KNSIConfigItemsStore : IBqlTable
	{
		#region StoreID
		public abstract class storeID : IBqlField { }
		[PXDBIdentity(IsKey = true), PXUIField(Enabled = false)]
		public virtual int? StoreID { get; set; }
		#endregion

		#region CompositeID
		public abstract class compositeID : IBqlField { }

		[PXDBInt(IsKey = true), PXUIField(DisplayName = "CompositeID")]	
		public virtual int? CompositeID { get; set; }
		#endregion

		#region MappedInventoryID
		public abstract class mappedInventoryID : IBqlField { }

		[PXUIField(DisplayName = "Inventory ID"), PXDefault, PXDBString(30, IsUnicode = true, InputMask = "", IsKey = true)]
		public virtual string MappedInventoryID { get; set; }
		#endregion

	}
}
