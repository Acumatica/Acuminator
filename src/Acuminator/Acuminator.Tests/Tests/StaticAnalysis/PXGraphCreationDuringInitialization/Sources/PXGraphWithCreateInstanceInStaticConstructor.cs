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
        static SWKMapadocCustomerExtensionMaint()
        {
            SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
            int key = maint.GetHashCode();
        }
    }

    public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
    {
    }
}
