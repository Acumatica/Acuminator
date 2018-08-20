using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderWithHold : SOOrderWithTotal
	{
		#region Hold
		public abstract class hold : IBqlField { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold")]
		public bool? Hold { get; set; }
		#endregion
	}
}
