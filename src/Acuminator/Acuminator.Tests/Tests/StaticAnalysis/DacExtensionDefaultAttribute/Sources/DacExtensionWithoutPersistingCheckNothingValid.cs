using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacExtensionDefaultAttribute.Sources
{
    public class MyOrderExt : PXCacheExtension<MyOrder>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault("E")]
        public int? OrderNbr { get; set; }
    }

    public class MyOrder : IBqlTable
    {
        [PXDBInt(IsKey = true)]
        public int? Id { get; set; }
        public abstract class id : IBqlField { }

        [PXDBString]
        [PXDefault("D")]
        public string OrderNbr { get; set; }
        public abstract class orderNbr : IBqlField { }
    }
}
