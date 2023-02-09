﻿using PX.Data;
using PX.SM;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
    public class SMAccessExtDerived : SMAccessExtBase
	{
        public override void Initialize()
        {
            int count = Base.Identities.Select().Count;

            if (count > 0)
            {
                Base.Identities.Cache.Insert(Base.Identities.Current);
                Base.Identities.Cache.Update(Base.Identities.Current);
                Base.Identities.Cache.Delete(Base.Identities.Current);

                Base.Identities.Insert(Base.Identities.Current);
                Base.Identities.Update(Base.Identities.Current);
                Base.Identities.Delete(Base.Identities.Current);
            }
        }
    }

	public class SMAccessExtBase : PXGraphExtension<SMAccessPersonalMaint>
	{
		public override void Initialize()
		{
		}
	}
}
