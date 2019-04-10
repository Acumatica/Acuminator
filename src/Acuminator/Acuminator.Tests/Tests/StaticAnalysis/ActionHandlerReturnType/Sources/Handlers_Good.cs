using PX.Data;
using System;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerReturnType.Sources
{
    public class SMUserProcess : PXGraph
    {
        public PXAction<PX.SM.Users> SyncMyUsers;

        public PXAction<PX.SM.Users> DontSyncYsers;

        [PXButton]
        [PXUIField]
        public IEnumerable syncMyUsers(PXAdapter adapter)
        {
            SyncUsers();
			return adapter.Get();
		}

        [PXButton]
        [PXUIField]
        public IEnumerable dontSyncUsers()
        {
            yield return null;
        }

        private void SyncUsers()
        {
            PXLongOperation.StartOperation(this, () => Console.WriteLine("Synced"));
        }
    }
}
