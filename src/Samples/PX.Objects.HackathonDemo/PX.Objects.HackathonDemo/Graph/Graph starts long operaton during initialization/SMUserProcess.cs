using PX.Data;
using System;

namespace PX.Objects.HackathonDemo
{
    public class SMUserProcess : PXGraph
    {
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
