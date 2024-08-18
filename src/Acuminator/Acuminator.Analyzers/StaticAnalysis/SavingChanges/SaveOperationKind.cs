#nullable enable

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
	internal enum SaveOperationKind
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

	internal enum PXDBOperationKind
	{
		None,

		/// <summary>
		/// PXDatabase.Insert or PXDatabase.Insert<> calls
		/// </summary>
		Insert,

		/// <summary>
		/// PXDatabase.Delete, PXDatabase.Delete<> or PXDatabase.ForceDelete calls
		/// </summary>
		Delete,

		/// <summary>
		/// PXDatabase.Update or PXDatabase.Update<> calls
		/// </summary>
		Update,

		/// <summary>
		/// PXDatabase.Ensure or PXDatabase.Ensure<> calls
		/// </summary>
		Ensure,
	}
}
