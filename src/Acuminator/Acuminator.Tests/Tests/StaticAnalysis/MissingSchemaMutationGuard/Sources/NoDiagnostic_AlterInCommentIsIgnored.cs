using PX.Data;

namespace Acuminator.Tests.Sources
{
	// The word ALTER appears only inside a SQL comment and a string literal —
	// neither should trigger the diagnostic.
	public class MyPlugin_AlterInCommentIsIgnored
	{
		public void UpdateDatabase()
		{
			PXDatabase.Execute(@"
                -- ALTER TABLE SOOrder ADD UsrPriority int NULL  (commented out)
                /* ALTER TABLE POOrder ADD UsrPriority int NULL  (block comment) */
                INSERT INTO UsrAuditLog (Message) VALUES ('ALTER TABLE is documented here')
            ");
		}
	}
}
