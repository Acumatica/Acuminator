using PX.Data;
using System;
using System.Collections;

namespace PX.Objects.HackathonDemo
{
    public class SMUserProcess : PXGraph
    {
        public PXSelect<SM.Users> Users;

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
