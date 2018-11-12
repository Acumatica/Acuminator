using PX.Data;
using PX.SM;
using System;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallingBaseActionHandler.Sources
{
    public class UserMaintExt : PXGraphExtension<UserMaint>
    {
        public delegate IEnumerable syncUsersDelegate(PXAdapter adapter);

        [PXOverride]
        [PXButton]
        [PXUIField(DisplayName = "Sync Users")]
        public virtual IEnumerable syncUsers(PXAdapter adapter, syncUsersDelegate baseMethod)
        {
            return Base.syncUsers(adapter);
        }
    }

    public class UserMaint : PXGraph<UserMaint, Users>
    {
        public PXSelect<Users> AllUsers;

        public PXAction<Users> SyncUsers;

        [PXButton]
        [PXUIField(DisplayName = "Sync Users")]
        public virtual IEnumerable syncUsers(PXAdapter adapter)
        {
            Console.WriteLine("Sync users");

            return adapter.Get();
        }
    }
}
