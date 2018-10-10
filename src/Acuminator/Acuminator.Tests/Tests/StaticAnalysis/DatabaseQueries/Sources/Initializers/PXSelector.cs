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
        private string _name;

        public override void Initialize()
        {
            _name = PXSelectorAttribute.Select<Users.displayName>(Base.Caches[typeof(Users)], new Users()) as string;
        }
    }

    public class UserEntry : PXGraph
    {
    }
}
