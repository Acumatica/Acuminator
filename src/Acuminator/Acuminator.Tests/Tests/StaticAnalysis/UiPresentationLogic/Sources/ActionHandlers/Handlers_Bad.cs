using PX.Data;
using PX.SM;
using System.Collections;
using System.Linq;

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
            Action1.SetVisible(false);
            Action1.SetEnabled(false);
            Action1.SetCaption("Action1");
            Action1.SetTooltip("Tooltip");
            PXUIFieldAttribute.SetVisible<Users.displayName>(AllUsers.Cache, null);
            PXUIFieldAttribute.SetVisibility<Users.displayName>(AllUsers.Cache, null, PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetEnabled<Users.displayName>(AllUsers.Cache, null);
            PXUIFieldAttribute.SetRequired<Users.displayName>(AllUsers.Cache, false);
            PXUIFieldAttribute.SetReadOnly<Users.displayName>(AllUsers.Cache, null);
            PXUIFieldAttribute.SetDisplayName<Users.displayName>(AllUsers.Cache, "Action1");
            PXUIFieldAttribute.SetNeutralDisplayName(AllUsers.Cache, nameof(Users.displayName), "Action1");

            var newList = new Users.state.List();

            PXStringListAttribute.SetList<Users.state>(AllUsers.Cache, null, newList);
            PXStringListAttribute.AppendList<Users.state>(AllUsers.Cache, null, newList.ValueLabelDic.Keys.ToArray(), newList.ValueLabelDic.Values.ToArray());
            PXStringListAttribute.SetLocalizable<Users.displayName>(AllUsers.Cache, null, false);
            PXIntListAttribute.SetList<Users.loginTypeID>(AllUsers.Cache, null, new[] { 0, 1 }, new[] { "A", "B" });
        }

        [PXButton]
        [PXUIField(DisplayName = "Action with Signature 2")]
        public IEnumerable action2(PXAdapter adapter)
        {
            Action2.SetVisible(false);
            Action2.SetEnabled(false);
            Action2.SetCaption("Action1");
            Action2.SetTooltip("Tooltip");
            PXUIFieldAttribute.SetVisible<Users.displayName>(AllUsers.Cache, null);
            PXUIFieldAttribute.SetVisibility<Users.displayName>(AllUsers.Cache, null, PXUIVisibility.Invisible);
            PXUIFieldAttribute.SetEnabled<Users.displayName>(AllUsers.Cache, null);
            PXUIFieldAttribute.SetRequired<Users.displayName>(AllUsers.Cache, false);
            PXUIFieldAttribute.SetReadOnly<Users.displayName>(AllUsers.Cache, null);
            PXUIFieldAttribute.SetDisplayName<Users.displayName>(AllUsers.Cache, "Action1");
            PXUIFieldAttribute.SetNeutralDisplayName(AllUsers.Cache, nameof(Users.displayName), "Action1");

            var newList = new Users.state.List();

            PXStringListAttribute.SetList<Users.state>(AllUsers.Cache, null, newList);
            PXStringListAttribute.AppendList<Users.state>(AllUsers.Cache, null, newList.ValueLabelDic.Keys.ToArray(), newList.ValueLabelDic.Values.ToArray());
            PXStringListAttribute.SetLocalizable<Users.displayName>(AllUsers.Cache, null, false);
            PXIntListAttribute.SetList<Users.loginTypeID>(AllUsers.Cache, null, new[] { 0, 1 }, new[] { "A", "B" });

            return adapter.Get();
        }
    }
}
