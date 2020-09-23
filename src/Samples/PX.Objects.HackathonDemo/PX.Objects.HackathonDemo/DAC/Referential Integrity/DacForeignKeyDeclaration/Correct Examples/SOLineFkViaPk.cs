using PX.Data;
using PX.Objects;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.HackathonDemo.ReferentialIntegrity.ForeignKeyExamples
{
	[PXCacheName("SO Line")]
	public partial class SOLineFkViaPk : IBqlTable
	{
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}

		public static class FK
		{
			public class SOOrderKey : SOOrder.PK.ForeignKeyOf<SOLine>.By<orderType, orderNbr> { }
		}

		public abstract class orderType : IBqlField { }

		public abstract class orderNbr : IBqlField { }

		public abstract class lineNbr : IBqlField { }
	}
}
