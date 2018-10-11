using PX.Data;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart.Sources.PXGraph
{
    public class SMUserMaintExt : PXGraphExtension<SMUserMaint>
    {
        public IEnumerable users()
        {
            ThrowExceptionMethod();
            
            return Enumerable.Empty<Users>();
        }

        private void ThrowExceptionMethod()
        {
            throw new PXSetupNotEnteredException("Setup not entered", typeof(Users));
        }
    }

    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;
    }
}
