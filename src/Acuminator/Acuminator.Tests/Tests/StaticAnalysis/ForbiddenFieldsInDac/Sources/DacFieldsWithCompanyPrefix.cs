using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.ForbiddenFieldsInDac.Sources
{
	/// <exclude/>
	[PXHidden]
	public partial class SomeOrder : IBqlTable
	{
		#region CompanyLocation
		public abstract class companyLocation : PX.Data.BQL.BqlString.Field<companyLocation> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Company Location")]
		public string CompanyLocation { get; set; }
		#endregion
		
		#region OrderCD
		public abstract class orderCD : PX.Data.BQL.BqlInt.Field<orderCD> { }
		[PXDefault]
		[PXUIField(DisplayName = "Order CD")]
		public int? OrderCD { get; set; }
		#endregion

		#region CompanyDescr
		public abstract class companyDescr : PX.Data.BQL.BqlString.Field<companyDescr> { }

		[PXDefault]
		[PXUIField(DisplayName = "Company Description")]
		public string CompanyDescr { get; set; }
		#endregion
	}
}