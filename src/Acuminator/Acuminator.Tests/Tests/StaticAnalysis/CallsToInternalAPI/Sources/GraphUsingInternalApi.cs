using System;
using PX.Data;
using PX.Common;

using static Acuminator.Tests.Tests.StaticAnalysis.CallsToInternalAPI.Sources.CommonHelpers;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallsToInternalAPI.Sources
{
	public class SOOrderMaintSync : PXGraph<SOOrderMaintSync>
	{
		private readonly ServiceProvider serviceProvider = new ServiceProvider();

		public int CountSyncReadyFiles(int filesNbr = 0)
		{
			if (WebConfig.IsClusterEnabled)
			{
				CommonHelpers.OnInitializing += (sender, e) => InvalidateCache();
			}

			return filesNbr;
		}

		public void InvalidateCache()
		{
			if (serviceProvider.AccessChecker != null && serviceProvider.AccessChecker.IsActive && serviceProvider.AccessChecker.ShouldCheck)
			{
				serviceProvider.AccessChecker.CheckAccess();
			}

			if (serviceProvider.Service != null && serviceProvider.Service.SomeFlag && serviceProvider.Service.IsActive)
			{
				serviceProvider.Service.ProvideService();
			}

			if (serviceProvider.DerivedService.IsDerived)
			{
				serviceProvider.DerivedService.ProvideService();
			}

			if (Helper.IsActive)
				Helper.DoSomething();

			PX.Api.Mobile.Legacy.Provider.InitCaches();
		}
	}
}
