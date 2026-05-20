using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_BareAlterTableAdd
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                ALTER TABLE SOOrder ADD UsrPriority int NULL
            ");
		}
	}
}
