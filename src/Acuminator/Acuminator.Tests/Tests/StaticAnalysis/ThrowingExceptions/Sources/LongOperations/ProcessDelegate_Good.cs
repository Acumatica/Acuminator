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
                throw new PXException();
            });
            OurUsers.SetProcessDelegate(user =>
            {
                throw new PXException();
            });
            OurUsers.SetProcessDelegate((Users user) => throw new PXException());

            OurUsers.SetProcessDelegate(ProcessItemList);
            OurUsers.SetProcessDelegate(delegate (List<Users> userList)
            {
                throw new PXException();
            });
            OurUsers.SetProcessDelegate(userList =>
            {
                throw new PXException();
            });
            OurUsers.SetProcessDelegate((List<Users> userList) => throw new PXException());
        }

        private static void ProcessItem(Users user)
        {
            throw new PXException();
        }

        private static void ProcessItemList(List<Users> userList)
        {
            throw new PXException();
        }
    }
}
