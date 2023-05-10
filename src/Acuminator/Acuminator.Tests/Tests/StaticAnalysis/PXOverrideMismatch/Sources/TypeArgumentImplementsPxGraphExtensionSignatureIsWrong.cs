using System;

namespace Acuminator.Tests.Sources
{
	public class SuperClass : PX.Data.PXGraphExtension<int>
	{
		public virtual object TestMethod(int x, bool drilldown)
		{
			return new object();
		}
	}

	public class BaseClass : PX.Data.PXGraphExtension<SuperClass>
	{
	}

	public class AuxiliaryExtension<T1, T2> : PX.Data.PXGraphExtension<T1, T2>
	{
	}

	public class ExtClass : AuxiliaryExtension<BaseClass, int>
	{
		[PX.Data.PXOverride]
		public virtual object TestMethod(int x, bool drilldown, double y, Func<int, bool, double, object> del)
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