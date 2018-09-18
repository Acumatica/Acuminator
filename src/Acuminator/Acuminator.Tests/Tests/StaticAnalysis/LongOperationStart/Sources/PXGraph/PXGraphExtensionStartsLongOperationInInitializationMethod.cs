using PX.Data;
using PX.SM;
using System;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
    public class SMAccessExt : PXGraphExtension<SMAccessPersonalMaint>
    {
        public override void Initialize()
        {
            int count = Base.Identities.Select().Count;

            if (count > 0)
            {
                PXLongOperation.StartOperation(this, () => Console.WriteLine("Long Operation has been started"));
            }
        }
    }
}
