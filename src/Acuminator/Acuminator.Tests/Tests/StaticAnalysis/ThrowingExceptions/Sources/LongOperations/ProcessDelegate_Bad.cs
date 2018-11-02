using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidViewUsageInProcessingDelegate.Sources
{
    public class UsersProcess : PXGraph<UsersProcess>
    {
        public PXCancel<Users> Cancel;

        public PXProcessing<Users, Where<Users.guest, Equal<False>>> OurUsers;

        public PXSetup<BlobProviderSettings> BolbSettings;

        public PXSelect<Users> AllUsers;

        public UsersProcess()
        {
            OurUsers.SetProcessAllCaption("Process users");
            OurUsers.SetProcessCaption("Process user");

            OurUsers.SetProcessDelegate(ProcessItem);
            OurUsers.SetProcessDelegate(delegate (Users user)
            {
                throw new PXSetupNotEnteredException<Users>(null);
            });
            OurUsers.SetProcessDelegate(user =>
            {
                throw new PXSetupNotEnteredException<Users>(null);
            });
            OurUsers.SetProcessDelegate((Users user) => throw new PXSetupNotEnteredException<Users>(null));

            OurUsers.SetProcessDelegate(ProcessItemList);
            OurUsers.SetProcessDelegate(delegate (List<Users> userList)
            {
                throw new PXSetupNotEnteredException<Users>(null);
            });
            OurUsers.SetProcessDelegate(userList =>
            {
                throw new PXSetupNotEnteredException<Users>(null);
            });
            OurUsers.SetProcessDelegate((List<Users> userList) => throw new PXSetupNotEnteredException<Users>(null));
        }

        private static void ProcessItem(Users user)
        {
            throw new PXSetupNotEnteredException<Users>(null);
        }

        private static void ProcessItemList(List<Users> userList)
        {
            throw new PXSetupNotEnteredException<Users>(null);
        }
    }
}
