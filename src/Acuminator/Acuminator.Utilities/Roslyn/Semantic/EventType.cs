namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// Represents an event type in Acumatica Event Model.
	/// </summary>
	public enum EventType : byte
	{
		None,

		CacheAttached,

		RowSelecting,
		RowSelected,
		RowInserting,
		RowInserted,
		RowUpdating,
		RowUpdated,
		RowDeleting,
		RowDeleted,
		RowPersisting,
		RowPersisted,

		FieldSelecting,
		FieldDefaulting,
		FieldVerifying,
		FieldUpdating,
		FieldUpdated,

		CommandPreparing,
		ExceptionHandling,
	}
}
