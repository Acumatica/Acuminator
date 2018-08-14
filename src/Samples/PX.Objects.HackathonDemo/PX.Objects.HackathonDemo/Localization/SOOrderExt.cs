using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class SOOrderExt : PXCacheExtension<SOOrder>
    {
        public const string SpecialOrderNbr = "SPECIAL";

        [PXString]
        [PXUIField(DisplayName = "Custom Text")]
        public string CustomText { get; set; }
        public abstract class customText : IBqlField { }
    }
}
