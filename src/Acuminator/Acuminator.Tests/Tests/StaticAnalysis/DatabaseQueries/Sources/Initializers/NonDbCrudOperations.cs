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
        public PXSelect<Users> AllUsers;

        public override void Initialize()
        {
            Users user = AllUsers.Cache.CreateInstance() as Users;

            AllUsers.Insert(user);
            AllUsers.Update(user);
            AllUsers.Delete(user);

            AllUsers.Cache.Insert(user);
            AllUsers.Cache.Update(user);
            AllUsers.Cache.Delete(user);
        }
    }

    public class UserEntry : PXGraph
    {
    }
}
