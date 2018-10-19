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

            OurUsers.SetParametersDelegate(ProcessParameters);
            OurUsers.SetParametersDelegate(delegate (List<Users> users)
            {
                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                return processingGraph.BlobSettings.Select().Count > 0;
            });
            OurUsers.SetParametersDelegate(users =>
            {
                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.Cancel.Press();

                return true;
            });
            OurUsers.SetParametersDelegate(users => PXGraph.CreateInstance<UsersProcess>().OurUsers.Select().Count > 0);
        }

        private static bool ProcessParameters(List<Users> users)
        {
            var processingGraph = PXGraph.CreateInstance<UsersProcess>();

            return processingGraph.Filter.Current == null;
        }
    }
}
