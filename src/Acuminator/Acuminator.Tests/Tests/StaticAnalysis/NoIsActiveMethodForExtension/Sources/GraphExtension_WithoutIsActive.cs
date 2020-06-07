using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension.Sources
{
	public sealed class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
	{
	}

	public sealed class SOOrderEntryExtDiscount : PXGraphExtension<SOOrderEntry>
	{
		public static void IsActive() { }

		public static bool IsActive<T>() => false;
	}

	public sealed class SOOrderEntryExt1 : PXGraphExtension<SOOrderEntry>
	{
		public void IsActive() { }
	}

	public sealed class SOOrderEntryExt2 : PXGraphExtension<SOOrderEntry>
	{
		public static int IsActive() => 0;

		public static bool IsActive(object obj) => true;
	}

	public sealed class SOOrderEntryExtSales1 : PXGraphExtension<SOOrderEntry>
	{
		public static bool IsActive => true;  //Property is not supported
	}



	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{

	}
}
