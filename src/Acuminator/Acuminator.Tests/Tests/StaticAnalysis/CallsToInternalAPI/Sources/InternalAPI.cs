using System;
using PX.Data;
using PX.Common;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallsToInternalAPI.Sources
{
	[PXInternalUseOnly]
	public class InternalService
	{
		public bool IsActive;

		public bool SomeFlag { get; }

		public void ProvideService()
		{

		}
	}

	public class DerivedService : InternalService
	{
		public bool IsDerived;
	}

	[PXInternalUseOnly]
	public class InternalHelpers
	{
		public static event EventHandler OnInitializing;
	}

	public class CommonHelpers : InternalHelpers
	{
		public static class Helper
		{
			public static bool IsActive { get; }

			public static void DoSomething() { }
		}
	}


	public class AccessChecker
	{
		[PXInternalUseOnly]
		public bool IsActive;

		[PXInternalUseOnly]
		public bool ShouldCheck { get; }

		[PXInternalUseOnly]
		public void CheckAccess()
		{

		}
	}


	public class ServiceProvider
	{
		public readonly InternalService Service = new InternalService();
		public readonly DerivedService DerivedService;

		public AccessChecker AccessChecker = new AccessChecker();

		public ServiceProvider()
		{
			DerivedService = new DerivedService();
		}
	}
}
