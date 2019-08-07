using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public partial class SOOrder : IBqlTable
	{
		#region CompanyID
		// Acuminator disable once PX1027 Description [Justification]
		public abstract class companyId : IBqlField { }
		// Acuminator disable once PX1027 Description [Justification]
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Company ID")]
		public string CompanyID { get; set; }
		#endregion
		#region OrderNbr
		public abstract class orderNbr : IBqlField { }
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr")]
		public int? OrderNbr { get; set; }
		#endregion
		#region  DeletedDatabaseRecord
		// Acuminator disable once PX1027 Description [Justification]
		public abstract class deletedDatabaseRecord { }
		// Acuminator disable once PX1027 Description [Justification]
		[PXDefault]
		[PXUIField(DisplayName = "Deleted Flag")]
		public string DeletedDatabaseRecord { get; set; }
		#endregion
		#region OrderCD
		public abstract class orderCD : IBqlField { }
		[PXDefault]
		[PXUIField(DisplayName = "Order CD")]
		public int? OrderCD { get; set; }
		#endregion
		#region CompanyMask
		// Acuminator disable once PX1027 Description [Justification]
		public abstract class companyMask : IBqlField { }
		// Acuminator disable once PX1027 Description [Justification]
		[PXDefault]
		[PXUIField(DisplayName = "Company Mask")]
		public string CompanyMask { get; set; }
		#endregion
	}
}