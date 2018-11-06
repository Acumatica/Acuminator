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

            OurUsers.SetParametersDelegate(ProcessParameters);
            OurUsers.SetParametersDelegate(delegate (List<Users> users)
            {
                Console.WriteLine("Users parameters processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();
                var result = processingGraph.AllUsers.Select().Count > 0;

                if (!result)
                {
                    throw new PXException();
                }

                return result;
            });
            OurUsers.SetParametersDelegate(users =>
            {
                Console.WriteLine("Users parameters processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();
                var result = processingGraph.AllUsers.Select().Count > 0;

                if (!result)
                {
                    throw new PXException();
                }

                return result;
            });
            OurUsers.SetParametersDelegate(users => throw new PXException());
        }

        private static bool ProcessParameters(List<Users> users)
        {
            Console.WriteLine("Users parameters processing");

            var processingGraph = PXGraph.CreateInstance<UsersProcess>();
            var result = processingGraph.AllUsers.Select().Count > 0;

            if (!result)
            {
                throw new PXException();
            }

            return result;
        }
    }
}
