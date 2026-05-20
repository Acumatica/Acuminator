using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_BareCreateIndex
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                CREATE INDEX IX_SOOrder_UsrPriority ON SOOrder(UsrPriority)
            ");
		}
	}
}
