using PX.Data;
using PX.SM;
using System.Collections;

namespace PX.Objects.HackathonDemo
{
    public class SMUserMaint : PXGraph<UserEntry, Users>
    {
        public PXSelect<Users> Users;

        public PXAction<Users> AddTooltip;

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
            Next.Press();
            Actions.PressSave();

            return new PXSelect<Users>(this).Select();
        }

        
        [PXUIField(DisplayName = "Add Tooltip")]
        public void addTooltip()
        {
            Next.SetTooltip("Next Action");
        }
    }
}
