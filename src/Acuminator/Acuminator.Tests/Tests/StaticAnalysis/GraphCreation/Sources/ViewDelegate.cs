using PX.Data;
using PX.SM;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges.Sources
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public IEnumerable users()
        {
            PXGraph g = PXGraph.CreateInstance<SMUserMaint>();

            return new PXSelect<Users>(g).Select();
        }
    }
}
