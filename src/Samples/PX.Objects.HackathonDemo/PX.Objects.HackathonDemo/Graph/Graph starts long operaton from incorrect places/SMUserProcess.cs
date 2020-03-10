using PX.Data;
using System;
using System.Collections;

namespace PX.Objects.HackathonDemo
{
    public class SMUserProcess : PXGraph
    {
        public PXSelect<PX.SM.Users> Users;

        public PXAction<PX.SM.Users> SyncMyUsers;

        [PXButton]
        [PXUIField]
        public virtual void syncMyUsers()
        {
            SyncUsers();
        }

        public IEnumerable users()
        {
            SyncUsers();

            return new PXSelect<PX.SM.Users>(this).Select();
        }

        public SMUserProcess()
        {
            SyncUsers();
        }

        private void SyncUsers()
        {
            PXLongOperation.StartOperation(this, () => Console.WriteLine("Synced"));
        }
    }
}
