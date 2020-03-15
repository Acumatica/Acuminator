using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacUiAttributes.Sources
{
    public class GoodDac : IBqlTable
    {
        public abstract class someField { }
        [PXDBString]
        [PXStringList(new[] { "O", "N" }, new[] { "Open", "New" })]
        public string SomeField { get; set; }
    }
}
