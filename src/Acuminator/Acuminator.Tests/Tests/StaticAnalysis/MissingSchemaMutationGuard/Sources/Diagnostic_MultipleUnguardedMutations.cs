using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_MultipleUnguardedMutations
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                ALTER TABLE SOOrder ADD UsrPriority int NULL;
                ALTER TABLE POOrder ADD UsrPriority int NULL;
            ");
		}
	}
}
