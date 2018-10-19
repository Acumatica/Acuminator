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
                Console.WriteLine("Users item processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.AllUsers.Update(user);
                processingGraph.Persist();
            });
            OurUsers.SetProcessDelegate(user =>
            {
                Console.WriteLine("Users item processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.AllUsers.Update(user);
                processingGraph.Persist();
            });
            OurUsers.SetProcessDelegate((Users user) => PXGraph.CreateInstance<UsersProcess>().AllUsers.Select().Clear());

            OurUsers.SetProcessDelegate(ProcessItemList);
            OurUsers.SetProcessDelegate(delegate (List<Users> userList)
            {
                Console.WriteLine("Users item list processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.AllUsers.Update(userList.First());
                processingGraph.Persist();
            });
            OurUsers.SetProcessDelegate(userList =>
            {
                Console.WriteLine("Users item list processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.AllUsers.Update(userList.First());
                processingGraph.Persist();
            });
            OurUsers.SetProcessDelegate((List<Users> userList) => PXGraph.CreateInstance<UsersProcess>().AllUsers.Select().Clear());
        }

        private static void ProcessItem(Users user)
        {
            Console.WriteLine("Users item processing");

            var processingGraph = PXGraph.CreateInstance<UsersProcess>();

            processingGraph.AllUsers.Update(user);
            processingGraph.Persist();
        }

        private static void ProcessItemList(List<Users> userList)
        {
            Console.WriteLine("Users item list processing");

            var processingGraph = PXGraph.CreateInstance<UsersProcess>();

            processingGraph.AllUsers.Update(userList.First());
            processingGraph.Persist();
        }
    }
}
