using PX.Data;
using PX.SM;
using System.Collections;

namespace PX.Objects.HackathonDemo
{
    public class UserEntrytExt : PXGraphExtension<UserEntry>
    {
        [PXButton]
        [PXUIField(DisplayName = "Sync Users")]
        public virtual IEnumerable syncUsers(PXAdapter adapter)
        {
            return Base.SyncUsers.Press(adapter);
        }
    }

    public class UserEntry : PXGraph<UserEntry, Users>
    {
        public PXSelect<Users> AllUsers;

        public PXAction<Users> SyncUsers;

        [PXButton]
        [PXUIField(DisplayName = "Sync Users")]
        public virtual IEnumerable syncUsers(PXAdapter adapter)
        {
            return adapter.Get();
        }
    }
}
