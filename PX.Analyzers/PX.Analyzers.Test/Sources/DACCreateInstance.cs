﻿using System;
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
	        var invoice = new APInvoice();
        }
    }

	public class APInvoice : APRegister
	{
	}

	public class APRegister : IBqlTable
	{
	}
}
