using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		internal virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y);
		}
	}

	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y) * 2;
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}