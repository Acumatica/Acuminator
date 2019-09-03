using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo.Suppression.Comments
{
	public partial class POOrder : IBqlTable
	{
		#region CompanyMask
		// Acuminator disable once PX1060 LegacyBqlField [Justification]
		public abstract class companyMask : IBqlField { }
		// Acuminator disable once PX1030 PXDefaultIncorrectUse [Justification]
		// Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [Justification]
		[PXDefault]
		[PXUIField(DisplayName = "Company Mask")]
		public string CompanyMask { get; set; }
		#endregion

		#region Total
		public abstract class total : IBqlField { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}
}