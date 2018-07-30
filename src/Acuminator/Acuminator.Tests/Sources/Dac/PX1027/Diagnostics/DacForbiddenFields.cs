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
        public abstract class companyId : IBqlField { }
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
        public abstract class deletedDatabaseRecord { }
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
        public abstract class companyMask : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Company Mask")]
        public string CompanyMask { get; set; }
        #endregion



    }

    public partial class SOOrder : IBqlTable
    {
        #region  DeletedDatabaseRecord
        public abstract class deletedDatabaseRecord : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Deleted Flag")]
        public string DeletedDatabaseRecord { get; set; }
        #endregion
        #region CompanyMask
        public abstract class companyMask : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Company Mask")]
        public string CompanyMask { get; set; }
        #endregion

    }
}
