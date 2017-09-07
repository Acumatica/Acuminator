using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class SOOrder : IBqlTable { }
	public sealed class SOOrderExt : PXCacheExtension<SOOrder> { }
}
