using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class DacWithNotAcumaticaAttribute : IBqlTable
	{
		public abstract class someField : IBqlField { }

		[PXDBInt]
		public int? SomeField { get; set; }

		public abstract class someAccoundField : IBqlField { }

		[PXDBInt]
		[System.ComponentModel.Description("Some Account field")]
		public int? SomeAccountField { get; set; }
	}
}