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

        public PXSelect<Users> AllUsers;

        public override void Initialize()
        {
            _count = AllUsers.View.SelectMulti().Count;
        }
    }

    public class UserEntry : PXGraph
    {
    }
}
