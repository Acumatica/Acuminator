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
        private readonly int _count;

        public UserEntry()
        {
            _count = PXSelect<Users>.Select(this).Count;
        }
    }
}
