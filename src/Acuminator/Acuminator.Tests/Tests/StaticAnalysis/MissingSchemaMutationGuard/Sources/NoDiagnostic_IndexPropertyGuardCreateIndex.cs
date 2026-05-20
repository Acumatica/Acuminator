using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_IndexPropertyGuardCreateIndex
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                IF INDEXPROPERTY(OBJECT_ID('SOOrder'), 'IX_SOOrder_UsrPriority', 'IndexID') IS NULL
                    CREATE INDEX IX_SOOrder_UsrPriority ON SOOrder(UsrPriority)
            ");
		}
	}
}
