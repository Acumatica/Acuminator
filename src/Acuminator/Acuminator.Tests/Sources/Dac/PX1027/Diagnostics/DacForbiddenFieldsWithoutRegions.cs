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
        
        public abstract class companyId : IBqlField { }
        [PXDBString(IsKey = true, InputMask = "")]
        [PXDefault]
        [PXUIField(DisplayName = "Company ID")]
        public string CompanyID { get; set; }
        

        public abstract class orderNbr : IBqlField { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr")]
        public int? OrderNbr { get; set; }


        public abstract class deletedDatabaseRecord { }
        [PXDefault]
        [PXUIField(DisplayName = "Deleted Flag")]
        public string DeletedDatabaseRecord { get; set; }


        public abstract class orderCD : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Order CD")]
        public int? OrderCD { get; set; }


        public abstract class companyMask : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Company Mask")]
        public string CompanyMask { get; set; }
        
    }

    public partial class SOOrder : IBqlTable
    {
        public abstract class deletedDatabaseRecord : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Deleted Flag")]
        public string DeletedDatabaseRecord { get; set; }

        public abstract class companyMask : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Company Mask")]
        public string CompanyMask { get; set; }

    }
}
