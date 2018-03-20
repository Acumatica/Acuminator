using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
    public class SOOrderEntry : PXGraph<SOOrderEntry>
    {
        public void Release()
        {
	        var invoice = (APInvoice)this.Caches[typeof(APInvoice)].CreateInstance();
        }
    }

	public class APInvoice : APRegister
	{
	}

	public class APRegister : IBqlTable
	{
	}
}
