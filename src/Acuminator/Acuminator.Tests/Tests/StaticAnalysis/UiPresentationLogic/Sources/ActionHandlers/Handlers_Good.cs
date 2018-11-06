using PX.Data;
using PX.SM;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.UiPresentationLogic.Sources.ActionHandlers
{
    public class UsersMaint : PXGraph<UsersMaint, Users>
    {
        PXSelect<Users> AllUsers;

        public PXAction<Users> Action1;
        public PXAction<Users> Action2;

        [PXButton]
        [PXUIField(DisplayName = "Action with Signature 1")]
        public void action1()
        {
            var newList = new Users.state.List();
        }

        [PXButton]
        [PXUIField(DisplayName = "Action with Signature 2")]
        public IEnumerable action2(PXAdapter adapter)
        {
            var newList = new Users.state.List();

            return adapter.Get();
        }
    }
}
