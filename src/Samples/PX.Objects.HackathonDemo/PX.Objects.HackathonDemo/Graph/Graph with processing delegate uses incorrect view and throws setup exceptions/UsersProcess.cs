using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.DependencyInjection;
using PX.SM;

namespace PX.Objects.HackathonDemo
{
    public class UsersProcess : PXGraph<UsersProcess, Users>, IGraphWithInitialization
    {
		private static readonly Dictionary<string, string> _mappings;

        public PXProcessing<Users,
               Where<Users.guest, Equal<False>>> OurUsers;

        public PXSelect<Users> AllUsers;

        public PXAction<Users> SyncUsers;

		static UsersProcess()
		{
			_mappings = new Dictionary<string, string>();
		}

		public UsersProcess()
        {
            OurUsers.SetProcessAllCaption("Process users");
            OurUsers.SetProcessCaption("Process user");

            OurUsers.SetProcessDelegate(delegate (Users user)
            {
                Console.WriteLine("Users item processing");

                var processingGraph = PXGraph.CreateInstance<UsersProcess>();

                processingGraph.AllUsers.Update(user);
                processingGraph.Persist();
            });
        }

		void IGraphWithInitialization.Initialize()
		{

		}

		[PXButton]
        [PXUIField(DisplayName = "Sync Users")]
        public void syncUsers()
        {
            PXLongOperation.StartOperation(UID, SyncUsersLongOperation);

            if (AllUsers.Select().Count == 0)
            {
                throw new PXSetupNotEnteredException<Users>(null);
            }
        }

        public static void SyncUsersLongOperation()
        {
            var graph = PXGraph.CreateInstance<UsersProcess>();

            if (graph.AllUsers.Select().Count == 0)
            {
                throw new PXSetupNotEnteredException<Users>(null);
            }
        }
	}
}
