using System;

namespace Acuminator.Tests.Sources
{
	public class BaseClass : PX.Data.PXGraphExtension<MyGraph>
	{
		public virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y);
		}
	}

	public class ExtClass : PX.Data.PXGraphExtension<BaseClass, MyGraph>
	{
		public virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y) * 2;
		}
	}

	public class MyGraph : PX.Data.PXGraph
	{

	}
}

namespace PX.Data
{
	public class PXOverrideAttribute : Attribute
	{
	}

	public abstract class PXGraph
	{
	}

	public abstract class PXGraphExtension
	{
	}

	public abstract class PXGraphExtension<Graph> : PXGraphExtension
		where Graph : PXGraph
	{
		internal Graph _Base;
	}

	public abstract class PXGraphExtension<Extension1, Graph> : PXGraphExtension
		where Graph : PXGraph
		where Extension1 : PXGraphExtension<Graph>
	{
		internal Extension1 MyExt;

		internal Graph MyGraph;
	}
}