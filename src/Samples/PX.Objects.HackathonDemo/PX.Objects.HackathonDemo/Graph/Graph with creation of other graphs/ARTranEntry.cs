using PX.Data;
using System.Collections;

namespace PX.Objects.HackathonDemo
{
    public class ARTranEntry : PXGraph<ARTranEntry>
    {
        public PXSelect<ARTran> ArTran;

        public ARTranEntry()
        {
            ARTranDetailsEntry maint = PXGraph.CreateInstance<ARTranDetailsEntry>();
            int key = maint.GetHashCode();
        }

        public IEnumerable arTran()
        {
            ARTranDetailsEntry maint = PXGraph.CreateInstance<ARTranDetailsEntry>();

            return new PXSelect<ARTran>(maint).Select();
        }

        public class ARTran : IBqlTable
        {
            [PXDBString(15, IsUnicode = true, IsKey = true)]
            [PXDBDefault]
            [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
            public virtual string RefNbr { get; set; }
            public abstract class refNbr : IBqlField { }
        }
    }

    public class ARTranDetailsEntry : PXGraph<ARTranDetailsEntry>
    {
    }
}
