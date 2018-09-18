using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges.Sources
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        private readonly int _icount;

        public PXSelect<Users> Users;

        public SMUserMaint()
        {
            _icount = Users.Select().Count;
        }
    }
}
