using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache.Sources.PXGraph
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public SMUserMaint()
        {
            int icount = Users.Select().Count;
        }
    }
}
