using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension.Sources
{
	[PXProtectedAccess]
	public abstract class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
	{
		[PXProtectedAccess]
		protected abstract void Foo(int param1, string param2);
	}

	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{
		protected void Foo(int param1, string param2) 
		{
		}
	}
}
