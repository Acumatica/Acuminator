using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_IfNotExistsSysColumns
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE Name = 'UsrPriority' AND Object_ID = OBJECT_ID('SOOrder')
                )
                    ALTER TABLE SOOrder ADD UsrPriority int NULL
            ");
		}
	}
}
