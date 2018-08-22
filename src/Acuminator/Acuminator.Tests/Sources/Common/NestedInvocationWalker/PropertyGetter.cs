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
			int count = bar.Count;
		}
	}

	public class Bar
	{
		public int Count
		{
			get
			{
				throw new NotSupportedException();
			}
		}
	}
}
