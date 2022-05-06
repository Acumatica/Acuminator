using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces.Sources
{
    public class SWKMapadocCustomerExtensionMaint : PXGraphExtension<SWKMapadocConnMaint>
    {
        public override void Initialize()
        {
            SWKMapadocConnMaint maint = PXGraph.CreateInstance<SWKMapadocConnMaint>();
            int key = maint.GetHashCode();
        }
    }

    public class SWKMapadocConnMaint : PXGraph<SWKMapadocConnMaint>
    {
    }
}
