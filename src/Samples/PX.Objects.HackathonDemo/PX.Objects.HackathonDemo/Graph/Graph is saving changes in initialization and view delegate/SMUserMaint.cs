using PX.Data;
using PX.SM;
using System.Collections;

namespace PX.Objects.HackathonDemo
{
    public class SMUserMaint : PXGraph<SMUserMaint, Users>
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

        public IEnumerable users()
        {
            Cancel.Press();

            return new PXSelect<Users>(this).Select();
        }
    }
}
