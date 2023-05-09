using System;

namespace Acuminator.Tests.Sources
{
	public class SuperBaseClass : PX.Data.PXGraphExtension<int, int>
	{
		public void TestMethod(int x, bool drilldown, double y)
		{
			return;
		}
	}

	public class BaseClass : PX.Data.PXGraphExtension<SuperBaseClass>
	{
		[PX.Data.PXOverride]
		public void TestMethod(int x, bool drilldown, double y)
		{
			return;
		}
	}

	public class ExtClass : PX.Data.PXGraphExtension<BaseClass, int>
	{
		public delegate void MyDelegate(int x, bool drilldown, double y);

		[PX.Data.PXOverride]
		public void TestMethod(int x, bool drilldown, double y, MyDelegate del)
		{
			return;
		}
	}
}

namespace PX.Data
{
	public abstract class PXGraphExtension<Graph>
	{
		internal Graph _Base;
	}

	public abstract class PXGraphExtension<Extension1, Graph>
	{
		internal Extension1 MyExt;

		internal Graph MyGraph;
	}

	public class PXOverrideAttribute : Attribute
	{
	}
}