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


        public abstract class orderNbr : IBqlField { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Order Nbr")]
        public int? OrderNbr { get; set; }


        public abstract class orderCD : IBqlField { }
        [PXDefault]
        [PXUIField(DisplayName = "Order CD")]
        public int? OrderCD { get; set; }

    }

    public partial class SOOrder : IBqlTable
    {

    }
}
