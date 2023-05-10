using System;

namespace Acuminator.Tests.Sources
{
	public class SuperBaseClass : PX.Data.PXGraphExtension<int, int>
	{
		public virtual object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class BaseClass : PX.Data.PXGraphExtension<SuperBaseClass>
	{
		[PX.Data.PXOverride]
		public virtual object TestMethod(int x, bool drilldown, double y, Func<int, bool, double, object> del)
		{
			return new object();
		}
	}

	public class ExtClass : PX.Data.PXGraphExtension<BaseClass, int>
	{
		[PX.Data.PXOverride]
		public object TestMethod(int x, bool drilldown, double y, Func<int, bool, double, object> del)
		{
			return new object();
		}
	}
}

namespace PX.Data
{
	public class PXOverrideAttribute : Attribute
	{
	}

	public abstract class PXGraphExtension
	{
	}

	public abstract class PXGraphExtension<Graph> : PXGraphExtension
	{
		internal Graph _Base;
	}

	public abstract class PXGraphExtension<Extension1, Graph> : PXGraphExtension
	{
		internal Extension1 MyExt;

		internal Graph MyGraph;
	}
}