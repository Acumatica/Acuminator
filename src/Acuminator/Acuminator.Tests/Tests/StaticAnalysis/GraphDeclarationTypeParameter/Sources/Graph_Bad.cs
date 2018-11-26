using PX.Data;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphTypeParameter.Sources
{
	public class UserMaint : PXGraph<CustomerMaint>
	{
	}

	public class CustomerMaint : PXGraph<UserMaint, Users>
	{
	}

	public class PCMaint : PXGraph<CustomerMaint, Users, Users.contactID>
	{
	}
}
