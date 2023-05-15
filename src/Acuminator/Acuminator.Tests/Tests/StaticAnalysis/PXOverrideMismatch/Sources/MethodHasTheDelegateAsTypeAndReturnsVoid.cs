using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual void TestMethod(int x, bool drilldown, double y)
		{
			return;
		}
	}

	public class SecondLevelExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public virtual void TestMethod(int x, bool drilldown, double y)
		{
			return;
		}
	}

	public class DerivedExtension : PXGraphExtension<SecondLevelExtension, MyGraph>
	{
		public delegate void MyDelegate(int x, bool drilldown, double y);

		[PXOverride]
		public void TestMethod(int x, bool drilldown, double y, MyDelegate del)
		{
			return;
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}