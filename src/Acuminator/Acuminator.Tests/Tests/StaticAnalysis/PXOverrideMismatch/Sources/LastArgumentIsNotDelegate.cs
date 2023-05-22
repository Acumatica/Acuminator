using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual int TestMethod(int x, string y)
		{
			return x + Convert.ToInt32(y);
		}
	}

	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public virtual int TestMethod(int x, string y, int z)
		{
			return x + Convert.ToInt32(y)* 2 + z;
		}

		[PXOverride]
		public virtual int TestMethod(int x, string y, int[] z)
		{
			return x + Convert.ToInt32(y) * 2 + z.Length;
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}