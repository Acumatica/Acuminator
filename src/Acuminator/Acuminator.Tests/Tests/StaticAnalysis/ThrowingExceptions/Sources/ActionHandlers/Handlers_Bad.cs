using PX.Data;
using PX.SM;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions.Sources.ActionHandlers
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
            throw new PXSetupNotEnteredException<Users>(null);
        }

        [PXButton]
        [PXUIField(DisplayName = "Action with Signature 2")]
        public IEnumerable action2(PXAdapter adapter)
        {
            throw new PXSetupNotEnteredException<Users>(null);
        }
    }
}
