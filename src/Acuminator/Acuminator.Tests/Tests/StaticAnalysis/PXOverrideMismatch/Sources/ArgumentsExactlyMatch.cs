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
		[PX.Data.PXOverride]
		public virtual int Add(int x, string y)
		{
			return x + Convert.ToInt32(y) * 2;
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