using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class SOOrder : IBqlTable
    {
        #region CompanyID
        
        
        #endregion
        #region  DeletedDatabaseRecord
        
        
        #endregion
        #region CompanyMask
        
        
        #endregion
        #region OrderNbr
        public abstract class orderNbr : IBqlField { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr")]
        public int? OrderNbr { get; set; }
        #endregion
        #region  DeletedDatabaseRecord
        
        
        #endregion
    }
}
