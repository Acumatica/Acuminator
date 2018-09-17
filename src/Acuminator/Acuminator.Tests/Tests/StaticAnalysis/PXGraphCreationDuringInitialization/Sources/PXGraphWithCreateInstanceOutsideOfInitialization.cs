using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Common;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationDuringInitialization.Sources
{
    public class SWKMapadocCustomerExtensionMaint : PXGraph<SWKMapadocCustomerExtensionMaint>
    {
        public int GetKey()
        {
            SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();

            return maint.GetHashCode();
        }
    }

    public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
    {
    }
}
