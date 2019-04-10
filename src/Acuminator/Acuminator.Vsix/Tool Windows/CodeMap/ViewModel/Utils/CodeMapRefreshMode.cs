using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.ChangesClassification;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{	
	/// <summary>
	/// Values that represent code map refresh modes.
	/// </summary>
	internal enum CodeMapRefreshMode
	{
		/// <summary>
		/// A mode in which code map shouldn't be refreshed.
		/// </summary>
		NoRefresh,

		/// <summary>
		/// A mode in which code map should be cleared.
		/// </summary>
		Clear,

		/// <summary>
		/// A mode in which code map should be recalculated.
		/// </summary>
		Recalculate
	}
}
