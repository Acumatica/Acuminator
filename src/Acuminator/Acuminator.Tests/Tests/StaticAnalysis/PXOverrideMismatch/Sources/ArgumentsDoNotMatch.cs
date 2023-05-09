using System;

namespace Acuminator.Tests.Sources
{
	public class BaseClass : PX.Data.PXGraphExtension<int>
	{
		public virtual int TestMethod(int x, string y)
		{
			return x + Convert.ToInt32(y);
		}
	}

	public class ExtClass : PX.Data.PXGraphExtension<BaseClass, int>
	{
		[PX.Data.PXOverride]
		public virtual int TestMethod(int x, int z, string y)
		{
			return x + Convert.ToInt32(y)* 2 + z;
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