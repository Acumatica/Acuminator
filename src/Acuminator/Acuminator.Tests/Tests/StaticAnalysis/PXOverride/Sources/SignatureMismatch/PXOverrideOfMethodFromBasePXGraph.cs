using System;
using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseExtension : PXGraphExtension<MyGraph>
	{
		/// Overrides <seealso cref="PXGraph.Persist()"/>
		[PXOverride]
		public void Persist(Action base_Persist)
		{
			base_Persist();
		}

		/// Overrides <seealso cref="PXGraph.Clear()"/>
		[PXOverride]
		public void Clear(Action base_Clear)
		{
			base_Clear();
		}
	}

	public class MyGraph : PXGraph<MyGraph> 
	{
	}
}