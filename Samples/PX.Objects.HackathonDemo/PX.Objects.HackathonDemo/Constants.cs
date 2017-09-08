using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.HackathonDemo
{
	public class Open : Constant<string>
	{
		public Open() : base("O")
		{
		}
	}

	public class SalesOrder : Constant<string>
	{
		public SalesOrder() : base("SO")
		{
		}
	}
}
