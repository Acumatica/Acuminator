using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
	enum PXDatabaseKind
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
	}
}
