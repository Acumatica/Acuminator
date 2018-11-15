using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphTypeParameter.Sources
{
	public class UserMaint : PXGraph<UserMaint>
	{
	}

	public class CustomerMaint : PXGraph<CustomerMaint, Users>
	{
	}

	public class PCMaint : PXGraph<PCMaint, Users, Users.contactID>
	{
	}
}
