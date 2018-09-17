using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphSavingChangesDuringInitialization.Sources
{
    public class SMAccessExt : PXGraphExtension<SMAccessPersonalMaint>
    {
        public override void Initialize()
        {
            int count = Base.Identities.Select().Count;

            if (count > 0)
            {
                Base.Identities.Delete(Base.Identities.Current);
                Base.Actions.PressSave();
            }
        }
    }
}
