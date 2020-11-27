using PX.Data;
using PX.Common;
using System;

namespace PX.Objects.HackathonDemo.Graph.InternalApiUsage
{
    [PXInternalUseOnly]
    public class InternalService 
    {
		public bool SomeFlag { get; }

        public void ProvideService()
		{

		}
    }


	[PXInternalUseOnly]
	public class AccessChecker
	{
		public bool SomeFlag;

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
