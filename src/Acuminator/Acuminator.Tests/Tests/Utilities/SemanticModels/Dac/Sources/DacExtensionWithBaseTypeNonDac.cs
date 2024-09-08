using System;

using PX.Data;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Dac.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class SOOrderExt : PXCacheExtension<SOOrder>
	{
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion

		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public string Descr { get; set; }
	}

	[PXCacheName("Sales Order")]
	public class SOOrder : BaseDac, IBqlTable
	{
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }


		public abstract class orderNbr : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
	}

	public abstract class BaseDac
	{
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
	}
}
