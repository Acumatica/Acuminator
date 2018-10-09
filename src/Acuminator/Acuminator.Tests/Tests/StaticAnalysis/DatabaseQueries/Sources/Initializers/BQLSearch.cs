using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries.Sources.Initializers
{
    public class UserEntryExt : PXGraphExtension<UserEntry>
    {
        private int _count;

        public override void Initialize()
        {
            _count = PXSelect<Users>.Search<Users.displayName>(Base, "admin").Count;
        }
    }

    public class UserEntry : PXGraph
    {
    }
}
