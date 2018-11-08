using PX.Data;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerAttributes.Sources
{
    public class UserMaint : PXGraph<UserMaint, Users>
    {
        public PXSelect<Users> AllUsers;

        public PXAction<Users> SyncUsers;

        [PXButton]
        [PXUIField]
        public IEnumerable syncUsers(PXAdapter adapter)
        {
            return adapter.Get();
        }
    }
}
