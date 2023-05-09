using System;

namespace Acuminator.Tests.Sources
{
	public class BaseClass : PX.Data.PXGraphExtension<int>
	{
		public virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y);
		}
	}

	public class ExtClass : PX.Data.PXGraphExtension<BaseClass, int>
	{
		public virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y) * 2;
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
}