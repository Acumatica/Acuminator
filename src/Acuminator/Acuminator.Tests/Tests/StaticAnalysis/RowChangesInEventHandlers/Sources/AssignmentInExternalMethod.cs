﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void _(Events.RowSelected<SOInvoice> e)
		{
			SOInvoice row = null;
			row = e.Row;
			SOHelpers.InitializeRow(row);
		}

		protected virtual void _(Events.FieldDefaulting<SOInvoice, SOInvoice.refNbr> e)
		{
			var row = e.Row;
			SOHelpers.InitializeRow(row);
		}

		protected virtual void _(Events.FieldVerifying<SOInvoice, SOInvoice.refNbr> e)
		{
			SOHelpers.InitializeRow(e.Row);
		}
	}

	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion	
	}

	public static class SOHelpers
	{
		public static void InitializeRow(SOInvoice row)
		{
			row.RefNbr = "<NEW>";
		}
	}
}