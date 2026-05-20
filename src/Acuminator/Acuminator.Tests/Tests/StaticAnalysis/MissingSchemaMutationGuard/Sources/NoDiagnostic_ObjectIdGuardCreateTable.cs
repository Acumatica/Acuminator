using PX.Data;

namespace Acuminator.Tests.Sources
{
	public class MyPlugin_ObjectIdGuardCreateTable
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                IF OBJECT_ID('UsrCustomLookup', 'U') IS NULL
                    CREATE TABLE UsrCustomLookup (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Code nvarchar(32) NOT NULL,
                        Description nvarchar(256) NULL
                    )
            ");
		}
	}
}
