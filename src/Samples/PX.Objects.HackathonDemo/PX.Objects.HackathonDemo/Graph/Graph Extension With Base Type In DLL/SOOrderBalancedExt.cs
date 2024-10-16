using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace PX.Objects.HackathonDemo
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SOOrderBalancedExt : PXGraphExtension<PX.Objects.SO.SOInvoiceEntry>
	{
		public override void Initialize()
		{
			base.Initialize();

			Base.Document.AllowInsert = false;
			Base.Document.AllowUpdate = false;
			Base.Document.AllowDelete = false;
			Base.Document.WhereAnd<Where<SOOrderWithHold.hold, Equal<False>>>();
		}
	}
}
