using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Plugin that calls PXDatabase.Execute with non-DDL SQL — no schema mutation.
	public class MyPlugin_NonSchemaMutation
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                INSERT INTO UsrAuditLog (EventTime, UserID, Action)
                VALUES (GETDATE(), 'system', 'plugin-init')
            ");
		}
	}
}
