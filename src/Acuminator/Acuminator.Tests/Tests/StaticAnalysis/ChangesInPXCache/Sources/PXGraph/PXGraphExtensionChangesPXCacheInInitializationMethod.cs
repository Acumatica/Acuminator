using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
    public class SMAccessExt : PXGraphExtension<SMAccessPersonalMaint>
    {
        public override void Initialize()
        {
            int count = Base.Identities.Select().Count;

            if (count > 0)
            {
                Base.Identities.Cache.Insert(Base.Identities.Current);
                Base.Identities.Cache.Update(Base.Identities.Current);
                Base.Identities.Cache.Delete(Base.Identities.Current);
            }
        }
    }
}
