using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public partial class POOrder : IBqlTable
    {
        #region CompanyID
        // Acuminator disable once PX1060 LegacyBqlField [Justification]
        // Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [Justification]
        public abstract class companyId : IBqlField { }

        // Acuminator disable once PX1027 ForbiddenFieldsInDacDeclaration [Justification]
        [PXDBString(IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Company ID")]
        public string CompanyID { get; set; }
        #endregion

        #region  DeletedDatabaseRecord
        public abstract class deletedDatabaseRecord { }
        [PXDefault]
        [PXUIField(DisplayName = "Deleted Flag")]
        public string DeletedDatabaseRecord { get; set; }
        #endregion

        #region OrderNbr
        public abstract class orderNbr : IBqlField { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr")]
        public int? OrderNbr { get; set; }
        #endregion
    }
}
