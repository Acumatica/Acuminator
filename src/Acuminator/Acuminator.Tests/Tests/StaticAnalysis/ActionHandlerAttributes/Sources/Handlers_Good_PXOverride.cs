using PX.Data;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerAttributes.Sources
{
	public class UserMaint : PXGraph<UserMaint, Users>
	{
		public PXSelect<Users> AllUsers;

		public PXAction<Users> SyncUsers1;

		public PXAction<Users> SyncUsers2;

		public PXAction<Users> SyncUsers3;

		[PXButton]
		[PXUIField]
		public IEnumerable syncUsers1(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField]
		[PXButton]
		public IEnumerable syncUsers2(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXButton]
		[PXUIField]
		public IEnumerable syncUsers3(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}

	public class UserMaintExt : PXGraphExtension<UserMaint>
	{
		[PXOverride]
		public IEnumerable syncUsers1(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}
}
