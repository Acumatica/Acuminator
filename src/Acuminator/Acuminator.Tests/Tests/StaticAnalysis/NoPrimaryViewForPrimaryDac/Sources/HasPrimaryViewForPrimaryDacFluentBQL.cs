using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL.Fluent;



namespace PX.Objects.HackathonDemo
{
	public class PRSVRepairPrice : IBqlTable { }

	public class PRSVRepairPriceDetails : IBqlTable { }

	public class PRSVRepairPriceMaint : PXGraph<PRSVRepairPriceMaint, PRSVRepairPrice>
	{
		public SelectFrom<PRSVRepairPrice>.View RepairPrices;

		public SelectFrom<PRSVRepairPriceDetails>.View RepairPriceDetails;
	}
}
