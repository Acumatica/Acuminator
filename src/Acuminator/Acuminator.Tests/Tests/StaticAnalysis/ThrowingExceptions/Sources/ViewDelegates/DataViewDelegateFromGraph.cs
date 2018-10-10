using PX.Data;
using PX.SM;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions.Sources.ViewDelegates
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public IEnumerable users()
        {
            throw new PXSetupNotEnteredException("Setup not entered", typeof(Users));
        }
    }
}
