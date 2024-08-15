using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AP;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Dac.Sources
{
	[PXHidden]
	public class APInvoice2 : APInvoice
	{
		public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}
}
