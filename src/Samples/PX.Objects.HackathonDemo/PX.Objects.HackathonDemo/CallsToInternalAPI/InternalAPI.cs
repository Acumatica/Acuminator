using PX.Data;
using PX.Common;
using System;

namespace PX.Objects.HackathonDemo.Graph.InternalApiUsage
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
		public InternalService Service = new InternalService();   

		public AccessChecker AccessChecker = new AccessChecker();
	}
}
