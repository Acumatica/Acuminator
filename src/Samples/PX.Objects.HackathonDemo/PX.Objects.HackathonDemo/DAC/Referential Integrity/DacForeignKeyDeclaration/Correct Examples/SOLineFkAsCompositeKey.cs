using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity.ForeignKeyExamples
{
	[PXCacheName("SO Line")]
	public partial class SOLineFkAsCompositeKey : PX.Data.IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}

		public static class FK
		{
			public class SOOrderFK : CompositeKey<
												  Field<orderType>.IsRelatedTo<SOOrder.orderType>,
												  Field<orderNbr>.IsRelatedTo<SOOrder.orderNbr>
											     >
											     .WithTablesOf<SOOrder, SOLine>
			{ }
		}

		public abstract class orderType : IBqlField { }

		public abstract class orderNbr : IBqlField { }

		public abstract class lineNbr : IBqlField { }
	}
}
