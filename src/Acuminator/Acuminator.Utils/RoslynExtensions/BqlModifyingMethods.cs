using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace Acuminator.Utils
{
	public static class BqlModifyingMethods
	{
		private class DummyDac : IBqlTable { }

		public static readonly HashSet<string> PXSelectbaseBqlModifiers = new HashSet<string>
		{
			nameof(PXSelectBase<DummyDac>.WhereAnd),
			nameof(PXSelectBase<DummyDac>.WhereNew),
			nameof(PXSelectBase<DummyDac>.WhereOr),
			nameof(PXSelectBase<DummyDac>.Join),		
		};

		public static readonly HashSet<string> BqlCommandInstanceBqlModifiers = new HashSet<string>
		{
			nameof(BqlCommand.WhereAnd),
			nameof(BqlCommand.WhereNew),
			nameof(BqlCommand.WhereOr),
			nameof(BqlCommand.AggregateNew),
			nameof(BqlCommand.OrderByNew)
		};

		public static readonly HashSet<string> BqlCommandStaticBqlModifiers = new HashSet<string>
		{
			nameof(BqlCommand.Compose),
			nameof(BqlCommand.CreateInstance),
			nameof(BqlCommand.AddJoinConditions),
			nameof(BqlCommand.AppendJoin),
			nameof(BqlCommand.NewJoin)
		};
	}
}
