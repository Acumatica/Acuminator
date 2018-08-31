using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn
{
	public enum EventType
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
