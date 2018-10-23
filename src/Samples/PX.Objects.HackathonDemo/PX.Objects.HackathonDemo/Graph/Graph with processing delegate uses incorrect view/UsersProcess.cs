using PX.Data;
using PX.SM;
using System;

namespace PX.Objects.HackathonDemo
{
    public class UsersProcess : PXGraph<UsersProcess>
    {
        public PXProcessing<Users,
               Where<Users.guest, Equal<False>>> OurUsers;

        public PXSelect<Users> AllUsers;

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
    }
}
