using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class SecondLevelExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public virtual object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class DerivedExtension : PXGraphExtension<SecondLevelExtension, MyGraph>
	{
		public delegate object MyDelegate(int x, bool drilldown, double y);

		[PXOverride]
		public object TestMethod(int x, bool drilldown, double y, MyDelegate del)
		{
			return new object();
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}