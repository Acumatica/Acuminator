using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
    public class UserEntry : PXGraph
    {
        static UserEntry()
        {
            PXGraph.InstanceCreated.AddHandler<UserEntry>(InstanceCreatedHandler);
        }

        private static void InstanceCreatedHandler(UserEntry graph)
        {
            int count = PXSelect<Users>.Select(graph).Count;
        }
    }
}
