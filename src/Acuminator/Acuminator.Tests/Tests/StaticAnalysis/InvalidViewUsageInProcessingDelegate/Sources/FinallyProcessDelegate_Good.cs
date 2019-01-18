using PX.Data;
using PX.SM;

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

            OurUsers.SetProcessDelegate<UsersProcess>(ProcessItem, FinallyProcess);
            OurUsers.SetProcessDelegate(
                delegate (UsersProcess graph, Users user)
                {
                    graph.BlobSettings.Select();
                },
                delegate (UsersProcess graph)
                {
                    graph.BlobSettings.Select();
                }
            );
            OurUsers.SetProcessDelegate(
                (UsersProcess graph, Users user) =>
                {
                    graph.Cancel.Press();
                },
                (UsersProcess graph) =>
                {
                    graph.Cancel.Press();
                });
            OurUsers.SetProcessDelegate(
                (UsersProcess graph, Users user) => graph.OurUsers.Select().Clear(),
                (UsersProcess graph) => graph.OurUsers.Select().Clear());
        }

        private static void ProcessItem(UsersProcess graph, Users user)
        {
            var processingGraph = PXGraph.CreateInstance<UsersProcess>();
            var current = processingGraph.Filter.Current;
        }

        private static void FinallyProcess(UsersProcess graph)
        {
            var current = graph.Filter.Current;
			var entryGraph = CreateInstance<UserEntry>();
			var entryCurrent = entryGraph.AllUsers.Current;
			var localView = new PXSelect<Users>(graph);
        }
    }

	public class UserEntry : PXGraph<UserEntry, Users>
	{
		public PXSelect<Users> AllUsers;
	}
}
