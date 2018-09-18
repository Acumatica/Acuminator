using PX.Data;
using PX.SM;
using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public SMUserMaint()
        {
            int icount = Users.Select().Count;

            if (icount > 0)
            {
                StartLongOperation();
            }
        }

        private void StartLongOperation()
        {
            PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
        }
    }
}
