using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacKeyFieldDeclaration.Sources
{
	[PXHidden]
	public class APRegister : IBqlTable
	{

		#region RegisterID
		public abstract class registerID : IBqlField { }

		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "ID")]
		public virtual int? RegisterID { get; set; }
		#endregion
	}



	[PXHidden]
	public class APInvoice : APRegister
	{
		#region OrderNbr
		public abstract class orderNbr : IBqlField { }

		[PXUIField(DisplayName = "Order Nbr")]
		[PXDefault]
		[PXDBString(30, IsUnicode = true, InputMask = "", IsKey = true)]
		public virtual string OrderNbr { get; set; }
		#endregion

		#region Description
		public abstract class description : IBqlField { }

		[PXUIField(DisplayName = "Decription")]
		[PXDBString(100, IsUnicode = true)]
		public virtual string Description { get; set; }
		#endregion
	}
}
