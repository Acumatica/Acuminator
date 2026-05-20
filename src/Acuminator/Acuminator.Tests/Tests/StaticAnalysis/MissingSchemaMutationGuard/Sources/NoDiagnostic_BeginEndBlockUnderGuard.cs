using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_BeginEndBlockUnderGuard
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                IF COL_LENGTH('SOOrder', 'UsrPriority') IS NULL
                BEGIN
                    ALTER TABLE SOOrder ADD UsrPriority int NULL;
                END
            ");
		}
	}
}
