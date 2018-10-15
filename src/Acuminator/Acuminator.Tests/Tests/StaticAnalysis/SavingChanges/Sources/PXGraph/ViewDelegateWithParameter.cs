using PX.Data;
using PX.SM;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges.Sources
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public IEnumerable users([PXString]string parameter)
        {
            Actions.PressSave();

            return new PXSelect<Users, Where<Users.displayName, Equal<Required<Users.displayName>>>>(this).Select(parameter);
        }
    }
}
