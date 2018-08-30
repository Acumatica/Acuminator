using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class SOOrderExt : PXCacheExtension<SOOrder>
    {
        public const string SpecialOrderNbr = "SPECIAL1";
        public const string SpecialOrderNbr2 = "SPECIAL2";
        public const string SpecialOrderNbr3 = "SPECIAL3";
        public const string SpecialOrderNbr4 = "SPECIAL4";

        [PXString]
        [PXUIField(DisplayName = "Custom Text")]
        public string CustomText { get; set; }
        public abstract class customText : IBqlField { }
    }
}
