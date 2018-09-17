using PX.Data;
using PX.SM;

namespace PX.Objects.HackathonDemo
{
    public class SMUserMaint : PXGraph<SMUserMaint>
    {
        public PXSelect<Users> Users;

        public SMUserMaint()
        {
            int icount = Users.Select().Count;

            if (icount > 1)
            {
                Users.Delete(Users.Current);
                Actions.PressSave();
            }
        }
    }
}
