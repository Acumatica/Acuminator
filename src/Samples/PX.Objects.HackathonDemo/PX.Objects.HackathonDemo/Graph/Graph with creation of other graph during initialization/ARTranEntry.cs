using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class ARTranEntry : PXGraph<ARTranEntry>
    {
        public ARTranEntry()
        {
            ARTranDetailsEntry maint = PXGraph.CreateInstance<ARTranDetailsEntry>();
            int key = maint.GetHashCode();
        }
    }

    public class ARTranDetailsEntry : PXGraph<ARTranDetailsEntry>
    {
    }
}
