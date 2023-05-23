using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		public virtual bool IsParameterNull<T>(T x)
		{
			return x is null;
		}
	}

	public class DerivedExtension : PXGraphExtension<BaseExtension, MyGraph>
	{
		[PXOverride]
		public bool IsParameterNull<T>(T x)
		{
			return false;
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{

	}
}