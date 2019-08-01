using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class CreateAccountsFilter : IBqlTable
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlString.Field<bAccountID> { }

		[PXDefault]
		[PXString(30, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Business Account ID", Required = true)]
		public virtual string BAccountID { get; set; }
		#endregion

		#region AccountName
		public abstract class accountName : PX.Data.BQL.BqlString.Field<accountName> { }
		protected string _AccountName;
		[PXDefault]
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Company Name", Required = true)]
		public virtual string AccountName
		{
			get { return _AccountName; }
			set { _AccountName = value; }
		}
		#endregion

		#region LinkContactToAccount
		public abstract class linkContactToAccount : PX.Data.BQL.BqlBool.Field<linkContactToAccount> { }
		[PXBool]
		[PXUIField(DisplayName = "Link Contact to Account", Visible = false, Enabled = false)]
		public virtual bool? LinkContactToAccount { get; set; }
		#endregion
	}

	public sealed class CreateAccountsFilterExt : PXCacheExtension<CreateAccountsFilter>
	{
		#region Selected
		public abstract class selected : IBqlField { }

		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected { get; set; }
		#endregion
	}
}