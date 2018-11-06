using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public enum GraphInstantiationType
	{
		None,

		/// <summary>
		/// <code>new PXGraph()</code>
		/// </summary>
		ConstructorOfBaseType,

		/// <summary>
		/// <code>new SpecificGraph()</code>
		/// </summary>
		ConstructorOfSpecificType,

		/// <summary>
		/// <code>PXGraph.CreateInstance&lt;TGraph&gt;</code>
		/// </summary>
		CreateInstance
	}
}
