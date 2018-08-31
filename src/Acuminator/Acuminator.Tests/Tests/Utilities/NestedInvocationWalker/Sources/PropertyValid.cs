using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator
{
	public class Foo
	{
		public void DoSomething()
		{
			var bar = new Bar();
			bar.Count = 5;
		}
	}

	public class Bar
	{
		private int _count;

		public int Count
		{
			get
			{
				throw new NotSupportedException();
			}
			set { _count = value; }
		}
	}
}
