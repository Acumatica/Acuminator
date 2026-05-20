using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_BareCreateTable
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                CREATE TABLE UsrCustomLookup (
                    Id int IDENTITY(1,1) PRIMARY KEY,
                    Code nvarchar(32) NOT NULL
                )
            ");
		}
	}
}
