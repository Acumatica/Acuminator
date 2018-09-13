using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
    class PX1001ClassWithField
    {
        private readonly PXGraph _field = new PX1001FieldGraph();
    }

    class PX1001FieldGraph : PXGraph<PX1001FieldGraph>
    {
    }
}
