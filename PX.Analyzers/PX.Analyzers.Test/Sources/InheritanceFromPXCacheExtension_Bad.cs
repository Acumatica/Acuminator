using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
    public class SOOrder : IBqlTable { }
	public class SOOrderExt : PXCacheExtension<SOOrder> { }
	public class SOOrderSuperExt : PXCacheExtension<SOOrderExt, SOOrder> { }
	public class SOOrderSecondLevelExt : SOOrderSuperExt { }
}
