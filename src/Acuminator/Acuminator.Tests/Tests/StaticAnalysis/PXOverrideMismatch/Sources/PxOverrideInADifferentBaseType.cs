using System;

namespace Acuminator.Tests.Sources
{
	public class BaseClass
	{
		public virtual object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class ExtClass : BaseClass
	{
		[PX.Data.PXOverride]
		public override object TestMethod(int x, bool drilldown, double y)
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
}