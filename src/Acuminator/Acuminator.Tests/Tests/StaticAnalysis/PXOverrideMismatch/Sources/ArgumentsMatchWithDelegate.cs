using System;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual object TestMethod(int x, bool drilldown)
		{
			return new object();
		}
	}

	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public virtual object TestMethod(int x, bool drilldown, Func<int, bool, object> del)
		{
			return new object();
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}