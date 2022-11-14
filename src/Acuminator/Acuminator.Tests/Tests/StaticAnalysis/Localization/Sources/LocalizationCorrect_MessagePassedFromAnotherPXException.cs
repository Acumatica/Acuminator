using PX.Data;

namespace Acuminator.Tests.Tests
{
	public class ExternalService
	{
		public void DoSomeWorkSafe()
		{
			try
			{
				DoSomeWork();
			}
			catch (PXException ex)
			{
				throw new PXSetPropertyException(ex.Message, PXErrorLevel.Error);  //no alert
			}
		}

		protected virtual void DoSomeWork()
		{

		}
	}
}
