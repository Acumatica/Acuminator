using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
	enum SaveOperationKind
	{
		None,

		/// <summary>
		/// PXGraph.Actions.PressSave or PXSave.Press calls
		/// </summary>
		PressSave,

		/// <summary>
		/// PXGraph.Persist call
		/// </summary>
		GraphPersist,

		/// <summary>
		/// PXCache.Persist, PXCache.PersistInserted, PXCache.PersistUpdated, PXCache.PersistDeleted calls
		/// </summary>
		CachePersist,
	}
}
