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

        public PXFilter<Users> Filter;

        public PXProcessing<Users, Where<Users.guest, Equal<False>>> OurUsers;

        public PXSetup<BlobProviderSettings> BlobSettings;

        public PXSelect<Users> AllUsers;

        public UsersProcess()
        {
            OurUsers.SetProcessAllCaption("Process users");
            OurUsers.SetProcessCaption("Process user");

            OurUsers.SetProcessDelegate(ProcessItem);
            OurUsers.SetProcessDelegate(delegate (Users user)
            {
                var processingGraph = PXGraph.CreateInstance<UsersProcess>();
                processingGraph.BlobSettings.Select();
            });
            OurUsers.SetProcessDelegate(user =>
            {
                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.Cancel.Press();
            });
            OurUsers.SetProcessDelegate((Users user) => PXGraph.CreateInstance<UsersProcess>().OurUsers.Select().Clear());

            OurUsers.SetProcessDelegate(ProcessItemList);
            OurUsers.SetProcessDelegate(delegate (List<Users> userList)
            {
                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.BlobSettings.Select();
            });
            OurUsers.SetProcessDelegate(userList =>
            {
                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.Cancel.Press();
            });
            OurUsers.SetProcessDelegate((List<Users> userList) => PXGraph.CreateInstance<UsersProcess>().OurUsers.Select().Clear());
        }

        private static void ProcessItem(Users user)
        {
            var processingGraph = PXGraph.CreateInstance<UsersProcess>();
            var current = processingGraph.Filter.Current;
        }

        private static void ProcessItemList(List<Users> userList)
        {
            var processingGraph = PXGraph.CreateInstance<UsersProcess>();
            var current = processingGraph.Filter.Current;
        }
    }
}
