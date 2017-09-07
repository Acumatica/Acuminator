using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
    public class PX1001ClassWithMethod : IPrefetchable
    {
        public void Prefetch()
        {
            var graph = new PX1001MethodGraph();
        }
    }

    public class PX1001MethodGraph : PXGraph<PX1001MethodGraph>
    {
    }
}
