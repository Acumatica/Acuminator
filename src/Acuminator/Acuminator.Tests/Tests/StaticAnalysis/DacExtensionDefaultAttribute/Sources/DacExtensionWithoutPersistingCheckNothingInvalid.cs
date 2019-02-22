using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacExtensionDefaultAttribute.Sources
{
    public class MySalesOrderExt : PXCacheExtension<MySalesOrder>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault("E")]
        public int? OrderNbr { get; set; }
    }

    public class MySalesOrder : IBqlTable
    {
        [PXDBInt(IsKey = true)]
        public int? Id { get; set; }
        public abstract class id : IBqlField { }

        [PXDBString]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public string OrderNbr { get; set; }
        public abstract class orderNbr : IBqlField { }
    }
}
