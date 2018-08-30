using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources
{
    [Serializable]
    [PXPrimaryGraph(typeof(CFHistoryMaint))]
    [PXCacheName("My DAC Class")]
    public class DacWithGraphUsageInAttribute : IBqlTable
    {
        [PXUIField(DisplayName = "Reference Nbr.")]
        [PXDBInt]
        public virtual int? RefNbr { get; set; }
        public abstract class refNbr { }
    }

    public class CFHistoryMaint : PXGraph<CFHistoryMaint>
    {
    }
}
