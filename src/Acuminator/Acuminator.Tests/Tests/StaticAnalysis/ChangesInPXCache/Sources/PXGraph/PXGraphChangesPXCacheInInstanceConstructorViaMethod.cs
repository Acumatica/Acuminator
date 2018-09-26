using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache.Sources.PXGraph
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public SMUserMaint()
        {
            int icount = Users.Select().Count;

            if (icount > 0)
            {
                ChangeCache();
            }
        }

        private void ChangeCache()
        {
            Users.Cache.Insert(Users.Current);
            Users.Cache.Update(Users.Current);
            Users.Cache.Delete(Users.Current);
        }
    }
}
