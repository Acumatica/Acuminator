using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.HackathonDemo
{
	[PXCacheName(CS.Messages.FeaturesSet)]
	[PXPrimaryGraph(typeof(FeaturesMaint))]
	[Serializable]
	public class FeaturesSet : IBqlTable 
	{
		#region LicenseID
		public abstract class licenseID : PX.Data.BQL.BqlString.Field<licenseID> { }

		[PXString(64, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "License ID", Visible = false)]
		public virtual string LicenseID { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(3)]
		[PXIntList(
			new int[] { 0, 1, 2, 3 },
			new string[] { "Validated", "Failed Validation", "Pending Validation", "Pending Activation" }
		)]
		[PXUIField(DisplayName = "Activation Status", Enabled = false)]
		public int? Status
		{
			get;
			set;
		}
		#endregion

		#region ValidUntill
		public abstract class validUntill : PX.Data.BQL.BqlDateTime.Field<validUntill> { }

		[PXDBDate()]
		[PXUIField(DisplayName = "Next Validation Date", Enabled = false, Visible = false)]
		public virtual DateTime? ValidUntill { get; set; }
		#endregion

		#region ValidationCode
		public abstract class validationCode : PX.Data.BQL.BqlString.Field<validationCode> { }

		[PXString(500, IsUnicode = true, InputMask = "")]
		public virtual string ValidationCode { get; set; }
		#endregion

		#region FinancialModule
		public abstract class financialModule : PX.Data.BQL.BqlBool.Field<financialModule> { }

		[Feature(true, null, typeof(Select<GL.GLSetup>), DisplayName = "Finance", Enabled = false)]
		[PXDefault]
		public virtual bool? FinancialModule { get; set; }
		#endregion
	}
}
