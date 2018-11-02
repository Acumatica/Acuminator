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

            OurUsers.SetProcessDelegate<UsersProcess>(ProcessItem, FinallyProcess);
            OurUsers.SetProcessDelegate(
                delegate (UsersProcess graph, Users user)
                {
                    throw new PXSetupNotEnteredException<Users>(null);
                },
                delegate (UsersProcess graph)
                {
                    throw new PXSetupNotEnteredException<Users>(null);
                }
            );
            OurUsers.SetProcessDelegate(
                (UsersProcess graph, Users user) =>
                {
                    throw new PXSetupNotEnteredException<Users>(null);
                },
                (UsersProcess graph) =>
                {
                    throw new PXSetupNotEnteredException<Users>(null);
                });
            OurUsers.SetProcessDelegate(
                (UsersProcess graph, Users user) => throw new PXSetupNotEnteredException<Users>(null),
                (UsersProcess graph) => throw new PXSetupNotEnteredException<Users>(null));
        }

        private static void ProcessItem(UsersProcess graph, Users item)
        {
            throw new PXSetupNotEnteredException<Users>(null);
        }

        private static void FinallyProcess(UsersProcess graph)
        {
            throw new PXSetupNotEnteredException<Users>(null);
        }
    }
}
