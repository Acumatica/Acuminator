using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_InterpolatedIdentifiers
	{
		public void UpdateDatabase()
		{
			string tableName = "SOOrder";
			string columnName = "UsrPriority";
			PXDatabase.Execute($"ALTER TABLE {tableName} ADD {columnName} int NULL");
		}
	}
}
