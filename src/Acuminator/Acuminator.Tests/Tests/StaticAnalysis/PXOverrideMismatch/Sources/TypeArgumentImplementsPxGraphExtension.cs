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
	}

	public class AuxiliaryExtension<T1, T2> : PXGraphExtension<T1, T2>
	{
	}

	public class DerivedExtension : AuxiliaryExtension<SecondLevelExtension, MyGraph>
	{
		[PXOverride]
		public virtual object TestMethod(int x, bool drilldown, double y, Func<int, bool, double, object> del)
		{
			return new object();
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}