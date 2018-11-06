using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions.Sources.LongOperations
{
    public class UserMaint : PXGraph<UserMaint, Users>
    {
        public PXAction<Users> LongrunAction;
        public void longrunAction()
        {
            PXLongOperation.StartOperation(UID, BackgroundOperation);
        }

        public static void BackgroundOperation()
        {
            throw new PXSetupNotEnteredException<Users>(null);
        }
    }
}
