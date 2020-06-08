using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension.Sources
{
	public class SOOrderEntryExt1 : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive() => true;
	}

	public class SOOrderEntryExt2 : PXGraphExtension<SOOrderEntry>
	{
		public static Boolean IsActive() => true;
	}




	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{

	}
}
