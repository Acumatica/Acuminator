using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_UnrelatedGuard
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OtherTable')
                    PRINT 'logging note';
                ALTER TABLE SOOrder ADD UsrPriority int NULL
            ");
		}
	}
}
