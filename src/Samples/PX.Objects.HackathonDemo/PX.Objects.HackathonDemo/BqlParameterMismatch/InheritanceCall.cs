using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestEntry2 : PXGraph<SOOrderTestEntry2>
	{
		CustomSelect select;

		public object Foo()
		{
			var result = select.SelectSingle(1);

			return this;
		}
	}
}
