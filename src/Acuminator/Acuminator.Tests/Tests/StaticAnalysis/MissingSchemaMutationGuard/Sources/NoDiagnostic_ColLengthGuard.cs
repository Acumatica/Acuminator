using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_ColLengthGuard
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                IF COL_LENGTH('SOOrder', 'UsrPriority') IS NULL
                    ALTER TABLE SOOrder ADD UsrPriority int NULL
            ");
		}
	}
}
